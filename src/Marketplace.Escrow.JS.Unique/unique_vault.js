const { ApiPromise, WsProvider, Keyring } = require('@polkadot/api');
const { Abi, ContractPromise } = require("@polkadot/api-contract");
const { hexToU8a } = require('@polkadot/util');
const { decodeAddress, encodeAddress } = require('@polkadot/util-crypto');
const delay = require('delay');
const config = require('./config');
const fs = require('fs');
const { v4: uuidv4 } = require('uuid');

var BigNumber = require('bignumber.js');
BigNumber.config({ DECIMAL_PLACES: 12, ROUNDING_MODE: BigNumber.ROUND_DOWN, decimalSeparator: '.' });

const { Client } = require('pg');
let dbClient = null;

const incomingTxTable = "NftIncomingTransaction";
const incomingQuoteTxTable = "QuoteIncomingTransaction";
const offerTable = "Offer";
const tradeTable = "Trade";
const outgoingQuoteTxTable = "QuoteOutgoingTransaction";
const outgoingTxTable = "NftOutgoingTransaction";
const uniqueBlocksTable = "UniqueProcessedBlock";
let adminAddress;

const rtt = require("./runtime_types.json");
const contractAbi = require("./market_metadata.json");

const quoteId = 2; // KSM

const blackList = [ 7395, 1745, 8587, 573, 4732, 3248, 6986, 7202, 6079, 1732, 6494, 7553, 6840, 4541, 2102, 3503, 6560, 4269, 2659, 3912, 3470, 6290, 5811, 5209, 8322, 1813, 7771, 2578, 2661, 2983, 2119, 3310, 1547, 1740, 3187, 8194, 4651, 6188, 2167, 3487, 3106, 6070, 3446, 2407, 5870, 3745, 6389, 3246, 9385, 9680, 6457, 8462, 2350, 3927, 2269, 8485, 6198, 6787, 2047, 2197, 2379, 2466, 2558, 2682, 2759, 2979, 4232, 4273, 8187, 8190, 2935, 2673, 5228, 7683, 2075, 9845, 1645, 3198, 7490, 3192, 7907, 3167, 858, 239, 7613, 2790, 7043, 5536, 8277, 1134, 6378, 2416, 2373, 2240, 3952, 5017, 4999, 5986, 3159, 6155, 9329, 6445, 2117, 3935, 6091, 7841, 8725, 5194, 5744, 8120, 5930, 578, 6171, 6930, 2180, 6212, 5963, 7097, 8774, 5233, 7978, 2938, 2364, 1823, 1840, 8672, 5616, 737, 6122, 8769, 615, 9729, 3489, 427, 9883, 8678, 6579, 1776, 7061, 873, 5324, 2390, 6187, 9517, 2321, 3390, 3180, 6692, 2129, 9854, 1572, 7412, 3966, 1302, 1145, 1067, 3519, 7387, 8314, 648, 219, 2055, 825, 1195
];

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

async function getUniqueConnection() {
  // Initialise the provider to connect to the node
  log(`Connecting to ${config.wsEndpoint}`);
  const wsProvider = new WsProvider(config.wsEndpoint);

  // Create the API and wait until ready
  const api = new ApiPromise({ 
    provider: wsProvider,
    types: rtt
  });

  api.on('disconnected', async (value) => {
    log(`disconnected: ${value}`);
    process.exit();
  });
  api.on('error', async (value) => {
    log(`error: ${value.toString()}`);
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

async function getLastHandledUniqueBlock() {
  const conn = await getDbConnection();
  const res = await conn.query(`SELECT * FROM public."${uniqueBlocksTable}" ORDER BY public."${uniqueBlocksTable}"."BlockNumber" DESC LIMIT 1;`)
  const lastBlock = (res.rows.length > 0) ? res.rows[0].BlockNumber : 0;
  return lastBlock;
}

async function addHandledUniqueBlock(blockNumber) {
  const conn = await getDbConnection();
  await conn.query(`INSERT INTO public."${uniqueBlocksTable}" VALUES ($1, now());`, [blockNumber]);
}

async function addIncomingNFTTransaction(address, collectionId, tokenId, blockNumber) {
  const conn = await getDbConnection();

  // Convert address into public key
  const publicKey = Buffer.from(decodeAddress(address), 'binary').toString('base64');

  await conn.query(`INSERT INTO public."${incomingTxTable}"("Id", "CollectionId", "TokenId", "Value", "OwnerPublicKey", "UniqueProcessedBlockId", "Status", "LockTime", "ErrorMessage") VALUES ($1, $2, $3, 0, $4, $5, 0, now(), '');`, 
    [uuidv4(), collectionId, tokenId, publicKey, blockNumber]);
}

async function addOffer(seller, collectionId, tokenId, quoteId, price) {
  const conn = await getDbConnection();

  // Convert address into public key
  const publicKey = Buffer.from(decodeAddress(seller), 'binary').toString('base64');

  //Id | CreationDate | CollectionId | TokenId | Price | Seller | Metadata | OfferStatus | SellerPublicKeyBytes | QuoteId
  await conn.query(`INSERT INTO public."${offerTable}"("Id", "CreationDate", "CollectionId", "TokenId", "Price", "Seller", "Metadata", "OfferStatus", "SellerPublicKeyBytes", "QuoteId") 
    VALUES ($1, now(), $2, $3, $4, $5, '', 1, $6, $7);`, 
    [uuidv4(), collectionId, tokenId, price, publicKey, decodeAddress(seller), quoteId]);
}

async function getOpenOfferId(collectionId, tokenId) {
  const conn = await getDbConnection();
  const res = await conn.query(`SELECT * FROM public."${offerTable}" WHERE "CollectionId" = ${collectionId} AND "TokenId" = ${tokenId} AND "OfferStatus" = 1;`);
  const id = (res.rows.length > 0) ? res.rows[0].Id : '';
  return id;
}

async function updateOffer(collectionId, tokenId, newStatus) {
  const conn = await getDbConnection();

  const id = await getOpenOfferId(collectionId, tokenId);
  console.log(`Looking up offer for ${collectionId}-${tokenId}`);
  console.log(`Updating offer ${id}`);

  // Only update active offer (should be one)
  await conn.query(`UPDATE public."${offerTable}" SET "OfferStatus" = ${newStatus} WHERE "Id" = '${id}'`);

  return id;
}

async function addTrade(offerId, buyer) {
  const conn = await getDbConnection();

  // Convert address into public key
  const publicKey = Buffer.from(decodeAddress(buyer), 'binary').toString('base64');

  await conn.query(`INSERT INTO public."${tradeTable}"("Id", "TradeDate", "Buyer", "OfferId") 
    VALUES ($1, now(), $2, $3);`, 
    [uuidv4(), publicKey, offerId]);
}

async function addOutgoingQuoteTransaction(quoteId, amount, recipient) {
  const conn = await getDbConnection();

  // Convert address into public key
  const publicKey = Buffer.from(decodeAddress(recipient), 'binary').toString('base64');

  // Id | Status | ErrorMessage | Value | QuoteId | RecipientPublicKey | WithdrawType
  // WithdrawType == 1 => Withdraw matched
  await conn.query(`INSERT INTO public."${outgoingQuoteTxTable}"("Id", "Status", "ErrorMessage", "Value", "QuoteId", "RecipientPublicKey", "WithdrawType") 
    VALUES ($1, 0, '', $2, $3, $4, 1);`, 
    [uuidv4(), amount, parseInt(quoteId), publicKey]);
}

async function setIncomingKusamaTransactionStatus(id, status, error = "OK") {
  const conn = await getDbConnection();

  // Get one non-processed Kusama transaction
  await conn.query(`UPDATE public."${incomingQuoteTxTable}" SET "Status" = $1, "ErrorMessage" = $2 WHERE "Id" = $3`, 
    [status, error, id]);
}

async function getIncomingKusamaTransaction() {
  const conn = await getDbConnection();

  // Get one non-processed incoming Kusama transaction
  // Id | Amount | QuoteId | Description | AccountPublicKey | BlockId | Status | LockTime | ErrorMessage
  const res = await conn.query(`SELECT * FROM public."${incomingQuoteTxTable}" 
    WHERE 
      "Status" = 0 
      AND "QuoteId" = 2 LIMIT 1
  `);

  let ksmTx = {
    id: '',
    amount: '0',
    sender: null
  };

  if (res.rows.length > 0) {
    let publicKey = res.rows[0].AccountPublicKey;

    try {
      if ((publicKey[0] != '0') || (publicKey[1] != 'x'))
        publicKey = '0x' + publicKey;

      // Convert public key into address
      const address = encodeAddress(hexToU8a(publicKey));
      
      ksmTx.id = res.rows[0].Id;
      ksmTx.sender = address;
      ksmTx.amount = res.rows[0].Amount;
    }
    catch (e) {
      setIncomingKusamaTransactionStatus(res.rows[0].Id, 2, e.toString());
      log(e, "ERROR");
    }
    
  }

  return ksmTx;
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

function sendTransactionAsync(sender, transaction) {
  return new Promise(async (resolve, reject) => {
    try {
      await transaction.signAndSend(sender, ({ events = [], status }) => {
        const transactionStatus = getTransactionStatus(events, status);

        if (transactionStatus === "Success") {
          log(`Transaction successful`);
          resolve(events);
        } else if (transactionStatus === "Fail") {
          log(`Something went wrong with transaction. Status: ${status}`);
          reject(events);
        }
      });
    } catch (e) {
      console.log('Error: ', e);
      reject(e);
    }
  });

}

async function registerQuoteDepositAsync(api, sender, depositorAddress, amount) {
  log(`${depositorAddress} deposited ${amount} in ${quoteId} currency`);

  const abi = new Abi(contractAbi);
  const contract = new ContractPromise(api, abi, config.marketContractAddress);

  const value = 0;
  const maxgas = 1000000000000;

  let amountBN = new BigNumber(amount);
  const tx = contract.tx.registerDeposit(value, maxgas, quoteId, amountBN.toString(), depositorAddress);
  await sendTransactionAsync(sender, tx);
}

async function registerNftDepositAsync(api, sender, depositorAddress, collection_id, token_id, blockNumber) {
  console.log(`${depositorAddress} deposited ${collection_id}, ${token_id}`);
  const abi = new Abi(contractAbi);
  const contract = new ContractPromise(api, abi, config.marketContractAddress);

  const value = 0;
  const maxgas = 1000000000000;

  // if (blackList.includes(token_id)) {
  //   log(`Blacklisted NFT received. Silently returning.`, "WARNING");
  //   return;
  // }

  // Save in the DB
  await addIncomingNFTTransaction(depositorAddress, collection_id, token_id, blockNumber);

  const tx = contract.tx.registerNftDeposit(value, maxgas, collection_id, token_id, depositorAddress);
  await sendTransactionAsync(sender, tx);
}

function beHexToNum(beHex) {
  const arr = hexToU8a(beHex);
  let strHex = '';
  for (let i=arr.length-1; i>=0; i--) {
    let digit = arr[i].toString(16);
    if (arr[i] <= 15) digit = '0' + digit;
    strHex += digit;
  }
  return new BigNumber(strHex, 16);
}

async function sendNftTxAsync(api, sender, recipient, collection_id, token_id) {
  const tx = api.tx.nft
    .transfer(recipient, collection_id, token_id, 0);
  await sendTransactionAsync(sender, tx);
}

async function scanNftBlock(api, admin, blockNum) {

  if (blockNum % 10 == 0) log(`Scanning Block #${blockNum}`);
  const blockHash = await api.rpc.chain.getBlockHash(blockNum);

  // Memo: If it fails here, check custom types
  const signedBlock = await api.rpc.chain.getBlock(blockHash);
  const allRecords = await api.query.system.events.at(blockHash);

  // console.log(`Reading Block ${blockNum} Transactions`);
  for (const ex of signedBlock.block.extrinsics) {
    const { _isSigned, _meta, method: { args, method, section } } = ex;

    const events = allRecords
      .filter(({ phase }) =>
        phase.isApplyExtrinsic &&
        phase.asApplyExtrinsic.eq(ex.index)
      )
      .map(({ event }) => `${event.section}.${event.method}`);
    if (events.includes('system.ExtrinsicSuccess')) {
      // This call is successful

      if ((section == "nft") && (method == "transfer") && (args[0] == admin.address.toString())) {

        // Check that transfer was actually successful:
        let { Owner } = await api.query.nft.nftItemList(args[1], args[2]);
        if (Owner == admin.address.toString()) {
          log(`NFT deposit from ${ex.signer.toString()} id (${args[1]}, ${args[2]})`, "RECEIVED");
    
          // Register NFT Deposit
          const deposit = {
            address: ex.signer.toString(),
            collectionId: args[1],
            tokenId: args[2]
          };
  
          try {
            await registerNftDepositAsync(api, admin, deposit.address, deposit.collectionId, deposit.tokenId, blockNum);
            log(`NFT deposit from ${deposit.address} id (${deposit.collectionId}, ${deposit.tokenId})`, "REGISTERED");
          } catch (e) {
            log(`NFT deposit from ${deposit.address} id (${deposit.collectionId}, ${deposit.tokenId})`, "FAILED TO REGISTER");
          }
  
        }
        else {
          log(`NFT deposit from ${ex.signer.toString()} id (${args[1]}, ${args[2]})`, "FAILED TX");
        }
  
      }
      else if ((section == "contracts") && (method == "call")) {
        try {
          log(`Contract call in block ${blockNum}: ${args[0].toString()}, ${args[1].toString()}, ${args[2].toString()}, ${args[3].toString()}`);
  
          let data = args[3].toString();
          log(`data = ${data}`);
          if (data.includes("261a7028")) {
            const tokenId = data[28] + data[29] + data[26] + data[27];
            const id = Buffer.from(tokenId, 'hex').readIntBE(0, 2).toString();
            log(`${ex.signer.toString()} bought ${id} in block ${blockNum} hash: ${blockHash}`);
            log(`${ex.signer.toString()} bought ${id} in block ${blockNum} hash: ${blockHash}`, "SUCCESS");
          }
  
          // Quote Deposit registration
          // 0x5eb1cb1f02000000000000000080c6a47e8d03000000000000000000f2536430fd61850d86660508a278f2cd5f7258ea1c1cf9491c5d848327c98121
  
          // Buy 3457 (0D81):
          // Block hash:   0x3a88c2efd7d7131f43f7771c3e50a98cd90cf9b1e8b83ed890207f98ea93495a
          // Block number: 1,384,383
          // Data:         0x261a70280400000000000000810d000000000000
  
          // Withdraw NFT/Amount
          // 0xb3f7898ecbda75ddf8f6cea3f584c9c46fb0fe7fbb645804c89261014b78ff22
          // 1,384,398
          // 0xe80efa690400000000000000860f0000…88e769c8628cc58d53fbe7f6fbc52b3e
  
          // Register NFT 4744 (0x1288 = 8812 LE) deposit
          // 0xe80efa6904000000000000008812000000000000d81f689c5d4aef49114017168ed953595ead394faa5acfd8877702693157620a
  
  
          // Ask call
          if (data.includes("7d02ceb8")) {
            log(`======== Ask Call`);
            //    Ask ID   collection       token            quote            price
            // 0x 7d02ceb8 0300000000000000 1200000000000000 0200000000000000 0080c6a47e8d03000000000000000000
            //    0        4                12               20               28
  
            if (data.substring(0,2) === "0x") data = data.substring(2);
            const collectionIdHex = "0x" + data.substring(8, 24);
            const tokenIdHex = "0x" + data.substring(24, 40);
            const quoteIdHex = "0x" + data.substring(40, 56);
            const priceHex = "0x" + data.substring(56);
            const collectionId = beHexToNum(collectionIdHex).toString();
            const tokenId = beHexToNum(tokenIdHex).toString();
            const quoteId = beHexToNum(quoteIdHex).toString();
            const price = beHexToNum(priceHex).toString();
            log(`${ex.signer.toString()} listed ${collectionId}-${tokenId} in block ${blockNum} hash: ${blockHash} for ${quoteId}-${price}`);
  
            await addOffer(ex.signer.toString(), collectionId, tokenId, quoteId, price);
          }
  
          // Buy call
          if (data.includes("151e67be03")) {
            // We expect 4 events here with 
            // [1] being QuoteWithdrawMatched and 
            // [2] being NFTWithdraw 
  
            log(`======== WithdrawNFT Event`);
            const buyerAddress = encodeAddress(hexToU8a(allRecords[1].topics[3].toString()));
            log(`NFT Buyer address: ${buyerAddress}`);
            const tokenIdBN = beHexToNum(allRecords[1].topics[1].toString());
            const tokenIdOffset = new BigNumber('0x200000000', 16);
            const tokenId = tokenIdBN.minus(tokenIdOffset); 
            log(`tokenId = ${tokenId}`);
            const collectionIdBN = beHexToNum(allRecords[1].topics[0].toString());
            const collectionIdOffset = new BigNumber('0x100000000', 16);
            const collectionId = collectionIdBN.minus(collectionIdOffset); 
            log(`collectionId = ${collectionId}`);
            
            log(`======== WithdrawUniqueMatched Event`);
            const sellerAddress = encodeAddress(hexToU8a(allRecords[2].topics[2].toString()));
            log(`NFT Seller address: ${sellerAddress}`);
            const priceBN = beHexToNum(allRecords[2].topics[0].toString());
            const priceOffset = new BigNumber('0x1000000000000000000000000000000', 16);
            const price = priceBN.minus(priceOffset);
            const quoteIdBN = beHexToNum(allRecords[2].topics[1].toString());
            const quoteIdOffset = new BigNumber('0x100000000', 16);
            const quoteId = quoteIdBN.minus(quoteIdOffset);
            log(`Price: ${quoteId} - ${price.toString()}`);
  
            // Update offer to done (status = 2 = Traded)
            const id = await updateOffer(collectionId, tokenId, 2);
  
            // Record trade
            await addTrade(id, buyerAddress);
  
            // Record outgoing quote tx
            await addOutgoingQuoteTransaction(quoteId, price.toString(), sellerAddress);
  
            // Execute NFT transfer to buyer
            await sendNftTxAsync(api, admin, buyerAddress.toString(), parseInt(collectionId), parseInt(tokenId));
          }
        }
        catch (e) {
          log(e, "ERROR");
        }
      }
    }
  }
}

async function handleUnique() {

  const api = await getUniqueConnection();
  const keyring = new Keyring({ type: 'sr25519' });
  const admin = keyring.addFromUri(config.adminSeed);
  adminAddress = admin.address.toString();
  log(`Escrow admin address: ${adminAddress}`);

  // Work indefinitely
  while (true) {

    // 1. Catch up with blocks
    const finalizedHashNft = await api.rpc.chain.getFinalizedHead();
    const signedFinalizedBlockNft = await api.rpc.chain.getBlock(finalizedHashNft);

    while (true) {
      // Get last processed block
      let blockNum = parseInt(await getLastHandledUniqueBlock()) + 1;

      try {
        if (blockNum <= signedFinalizedBlockNft.block.header.number) {
          await addHandledUniqueBlock(blockNum);
          
          // Handle NFT Deposits (by analysing block transactions)
          await scanNftBlock(api, admin, blockNum);
        } else break;

      } catch (ex) {
        log(ex);
        if (!ex.toString().includes("State already discarded"))
          await delay(1000);
      }
    }

    // Handle queued KSM deposits
    let deposit = false;
    do {
      deposit = false;
      const ksmTx = await getIncomingKusamaTransaction();
      if (ksmTx.id.length > 0) {
        deposit = true;

        try {
          await registerQuoteDepositAsync(api, admin, ksmTx.sender, ksmTx.amount);
          await setIncomingKusamaTransactionStatus(ksmTx.id, 1);
          log(`Quote deposit from ${ksmTx.sender} amount ${ksmTx.amount.toString()}`, "REGISTERED");
        } catch (e) {
          log(`Quote deposit from ${ksmTx.sender} amount ${ksmTx.amount.toString()}`, "FAILED TO REGISTER");
        }
      }

    } while (deposit);
    

  }

}

async function main() {
  await handleUnique();
}

main().catch(console.error).finally(() => process.exit());
