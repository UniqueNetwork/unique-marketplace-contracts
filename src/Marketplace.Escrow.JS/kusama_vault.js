const { ApiPromise, WsProvider, Keyring } = require('@polkadot/api');
const delay = require('delay');
const config = require('./config');
const { decodeAddress, encodeAddress } = require('@polkadot/util-crypto');
const { v4: uuidv4 } = require('uuid');

var BigNumber = require('bignumber.js');
BigNumber.config({ DECIMAL_PLACES: 12, ROUNDING_MODE: BigNumber.ROUND_DOWN, decimalSeparator: '.' });

const { Client } = require('pg');
let dbClient = null;

const FEE = 0.1;
const incomingTxTable = "QuoteIncomingTransaction";
const outgoingTxTable = "QuoteOutgoingTransaction";
const kusamaBlocksTable = "KusamaProcessedBlock";
let adminAddress;

function getTime() {
  var a = new Date();
  var hour = a.getHours();
  var min = a.getMinutes();
  var sec = a.getSeconds();
  var time = `${hour}:${min}:${sec}`;
  return time;
}

function getDay() {
  var a = new Date();
  var year = a.getFullYear();
  var month = a.getMonth()+1;
  var date = a.getDate();
  var time = `${year}-${month}-${date}`;
  return time;
}

function log(operation, status = "") {
  console.log(`${getDay()} ${getTime()}: ${operation}${status.length > 0?',':''}${status}`);
}

async function getKusamaConnection() {
  // Initialise the provider to connect to the node
  const wsProvider = new WsProvider(config.wsEndpoint);

  // Create the API and wait until ready
  const api = new ApiPromise({ provider: wsProvider });

  api.on('disconnected', async (value) => {
    log(`disconnected: ${value}`, "ERROR");
    process.exit();
  });
  api.on('error', async (value) => {
    log(`error: ${value}`, "ERROR");
    process.exit();
  });

  await api.isReady;

  return api;
}

async function getDbConnection() {
  if (!dbClient) {
    dbClient = new Client({
      user: config.dbUser,
      host: config.dbHost,
      database: config.dbName,
      password: config.dbPassword,
      port: config.dbPort
    });
    dbClient.connect();
    log("Connected to the DB");
  }
  return dbClient;
}

async function getLastHandledKusamaBlock() {
  const conn = await getDbConnection();
  const res = await conn.query(`SELECT * FROM public."${kusamaBlocksTable}" ORDER BY public."${kusamaBlocksTable}"."BlockNumber" DESC LIMIT 1;`)
  const lastBlock = (res.rows.length > 0) ? res.rows[0].BlockNumber : 0;
  return lastBlock;
}

async function addHandledKusamaBlock(blockNumber) {
  const conn = await getDbConnection();
  await conn.query(`INSERT INTO public."${kusamaBlocksTable}" VALUES ($1, now());`, [blockNumber]);
}

function toHexString(byteArray) {
  let hex = '';
  for (let i=0; i<byteArray.length; i++) {
    hex += byteArray[i].toString(16).padStart(2, '0');
  }
  return hex;
}

async function addIncomingKusamaTransaction(amount, address, blockNumber) {
  const conn = await getDbConnection();

  // Convert address into public key
  const publicKey = toHexString(decodeAddress(address));

  await conn.query(`INSERT INTO public."${incomingTxTable}"("Id", "Amount", "QuoteId", "Description", "AccountPublicKey", "BlockId", "Status", "LockTime", "ErrorMessage") VALUES ($1, $2, 2, \'\', $3, $4, 0, null, null);`, 
    [uuidv4(), amount, publicKey, blockNumber]);
}

async function setOutgoingKusamaTransactionStatus(id, status, error = "OK") {
  const conn = await getDbConnection();

  // Get one non-processed Kusama transaction
  await conn.query(`UPDATE public."${outgoingTxTable}" SET "Status" = $1, "ErrorMessage" = $2 WHERE public."${outgoingTxTable}"."Id" = $3`, 
    [status, error, id]);
}

async function getOutgoingKusamaTransaction() {
  const conn = await getDbConnection();

  // Get one non-processed Kusama transaction
  const res = await conn.query(`SELECT * FROM public."${outgoingTxTable}" WHERE public."${outgoingTxTable}"."Status" = 0 AND public."${outgoingTxTable}"."QuoteId" = 2 LIMIT 1`);

  let ksmTx = {
    id: '',
    amount: '0',
    recipient: null,
    withdrawType: 0
  };

  if (res.rows.length > 0) {
    // Decode from base64
    let publicKey = Buffer.from(res.rows[0].RecipientPublicKey, 'base64');

    try {
      // Convert public key into address
      const address = encodeAddress(publicKey);
      
      ksmTx.id = res.rows[0].Id;
      ksmTx.recipient = address;
      ksmTx.amount = res.rows[0].Value;
      ksmTx.withdrawType = res.rows[0].WithdrawType;
    }
    catch (e) {
      setOutgoingKusamaTransactionStatus(res.rows[0].Id, 2, e.toString());
      log(e, "ERROR");
    }
    
  }

  return ksmTx;
}


async function scanKusamaBlock(api, blockNum) {
  if (blockNum % 10 == 0) log(`Scanning Block #${blockNum}`);
  const blockHash = await api.rpc.chain.getBlockHash(blockNum);

  const signedBlock = await api.rpc.chain.getBlock(blockHash);
  const allRecords = await api.query.system.events.at(blockHash);

  await signedBlock.block.extrinsics.forEach(async (ex, index) => {
    let { _isSigned, _meta, method: { args, method, section } } = ex;
    if (method == "transferKeepAlive") method = "transfer";
    if ((section == "balances") && (method == "transfer") && (args[0] == adminAddress)) {
      const events = allRecords
        .filter(({ phase }) =>
          phase.isApplyExtrinsic &&
          phase.asApplyExtrinsic.eq(index)
        )
        .map(({ event }) => `${event.section}.${event.method}`);

      if (events.includes('system.ExtrinsicSuccess')) {
        log(`Quote deposit in block ${blockNum} from ${ex.signer.toString()} amount ${args[1]}`, "RECEIVED");
  
        // Register Quote Deposit (save to DB)
        const amount = args[1];
        const address = ex.signer.toString();

        // Remove 10% fee
        const addFee = (new BigNumber(FEE)).plus(1);
        const amountBN = (new BigNumber(amount))
          .dividedBy(addFee)
          .integerValue(BigNumber.ROUND_UP); // 1.10        

        await addIncomingKusamaTransaction(amountBN.toString(), address, blockNum);
      }
      else {
        log(`Quote deposit from ${ex.signer.toString()} amount ${args[1]}`, "FAILED");
      }
  
    }
  });

}

function getTransactionStatus(events, status) {
  if (status.isReady) {
    return "NotReady";
  }
  if (status.isBroadcast) {
    return "NotReady";
  } 
  if (status.isInBlock || status.isFinalized) {
    if(events.filter(e => e.event.data.method === 'ExtrinsicFailed').length > 0) {
      return "Fail";
    }
    if(events.filter(e => e.event.data.method === 'ExtrinsicSuccess').length > 0) {
      return "Success";
    }
  }

  return "Fail";
}

function sendTxAsync(sender, transaction) {
  return new Promise(async (resolve, reject) => {
    try {
      let unsub = await transaction.signAndSend(sender, ({ events = [], status }) => {
        const transactionStatus = getTransactionStatus(events, status);

        if (transactionStatus === "Success") {
          log(`Transaction successful`);
          resolve(events);
          unsub();
        } else if (transactionStatus === "Fail") {
          log(`Something went wrong with transaction. Status: ${status}`);
          reject(events);
          unsub();
        }
      });
    } catch (e) {
      log('Error: ' + e.toString(), "ERROR");
      reject(e);
    }
  });
}

async function withdrawAsync(api, sender, recipient, amount, withdrawType) {

  // Check if market commission is big enough to pay transaction fee. If not, return the amount + commission - tx fee.
  let amountBN = new BigNumber(amount);
  let marketFee = (withdrawType == 0) ? new BigNumber(0) : amountBN.dividedBy(51.001).integerValue(BigNumber.ROUND_DOWN); // We received 102% of price, so the fee is 2/102 = 1/51 (+0.001 for rounding errors)
  const totalBalanceObj = await api.query.system.account(sender.address)
  const totalBalance = new BigNumber(totalBalanceObj.data.free);
  log(`amountBN = ${amountBN.toString()}`);
  log(`Market fee = ${marketFee.toString()}`);
  log(`Total escrow balance = ${totalBalance.toString()}`);
  let additionalMarketFee = new BigNumber(0); // in case if marketFee is insufficient

  let balanceTransaction;
  let feesSatisfied = false;
  while (!feesSatisfied) {
    balanceTransaction = api.tx.balances.transfer(recipient, amountBN.toString());
    const info = await balanceTransaction.paymentInfo(sender);
    const networkFee = new BigNumber(info.partialFee);
    log(`networkFee = ${networkFee.toString()}`);
  
    feesSatisfied = true;
    if (networkFee.isGreaterThan(marketFee.plus(additionalMarketFee))) {
      additionalMarketFee = networkFee.minus(marketFee);
      amountBN = amountBN.minus(additionalMarketFee);
      log(`Market fee ${marketFee.toString()} is insufficient to pay network fee of ${networkFee.toString()}. Will only send ${amountBN.toString()}`);
      feesSatisfied = false;
    }
    // Check that total escrow balance is enough to send this amount
    if (totalBalance.minus(marketFee).isLessThan(amountBN)) {
      log(`Escrow balance ${totalBalance.toString()} is insufficient to send ${amountBN.toString()}. Will only send ${totalBalance.minus(networkFee).toString()}.`);
      amountBN = totalBalance.minus(networkFee);
      feesSatisfied = false;
    }

    if (amountBN.isLessThan(0)) {
      log(`Withdraw is too small. Will not process.`);
      throw "Withdrawal is too small";
    }
  }

  await sendTxAsync(sender, balanceTransaction);
}

async function handleKusama() {

  const api = await getKusamaConnection();
  const keyring = new Keyring({ type: 'sr25519', addressPrefix: 2 });
  keyring.setSS58Format(2);
  const admin = keyring.addFromUri(config.adminSeed);
  adminAddress = admin.address.toString();
  log(`Escrow admin address: ${adminAddress}`);

  // Work indefinitely
  while (true) {
    // 1. Catch up with blocks
    const finalizedHash = await api.rpc.chain.getFinalizedHead();
    const signedFinalizedBlock = await api.rpc.chain.getBlock(finalizedHash);
    while (true) {
      try {
        // Get the last processed block
        let lastKusamaBlock = parseInt(await getLastHandledKusamaBlock());

        if (lastKusamaBlock + 1 <= parseInt(signedFinalizedBlock.block.header.number)) {
          lastKusamaBlock++;

          // Handle Kusama Deposits (by analysing block transactions)
          await scanKusamaBlock(api, lastKusamaBlock);
          await addHandledKusamaBlock(lastKusamaBlock);
        } else break;

      } catch (ex) {
        log(ex, "ERROR");
        await delay(1000);
      }
    }

    // 2. Handle queued withdrawals
    let withdrawal = false;
    do {
      withdrawal = false;
      const ksmTx = await getOutgoingKusamaTransaction();
      if (ksmTx.id.length > 0) {
        withdrawal = true;

        try {
          // Handle withdrawals by type (withdraw or match )
          let withdrawType = ksmTx.withdrawType;
          let amountBN = new BigNumber(ksmTx.amount);
          let amountReturned = amountBN;
          if (withdrawType == 0) {
            // Withdraw unused => return commission
            // Add 2% fee to the returned amount
            amountReturned = amountBN.multipliedBy(51.001).dividedBy(50.001).integerValue(BigNumber.ROUND_DOWN);
          }
          log(`Quote withdraw (${(withdrawType == 0)?"unused":"matched"}): ${ksmTx.recipient.toString()} withdarwing amount ${amountReturned.toString()}`, "START");

          // Set status before handling (safety measure)
          await setOutgoingKusamaTransactionStatus(ksmTx.id, 1);
          await withdrawAsync(api, admin, ksmTx.recipient, amountReturned.toString(), withdrawType);
        }
        catch (e) {
          await setOutgoingKusamaTransactionStatus(ksmTx.id, 2, e);
        }
        finally {
          log(`Quote withdraw: ${ksmTx.recipient.toString()} withdarwing amount ${ksmTx.amount}`, "END");
        }
      }
    } while (withdrawal);

    await delay(1000);
  }

}

async function main() {
  await handleKusama();
}

main().catch(console.error).finally(() => process.exit());

