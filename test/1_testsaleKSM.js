const { assert } = require("console");
require('dotenv').config();
const owner =  process.env.OWNER;
const escrow =  process.env.ESCROW;

const MarketPlace = artifacts.require('MarketPlace.sol');

const Token721 = artifacts.require('ERC721example.sol');



contract ("MarketPlace for KSM", accounts => {
    var mpKSM, t721;
    const idNFT = 1234;
    const price = 1000;
    const depoSum = 1500;
    const addrKSM = web3.utils.toChecksumAddress("0x0000000000000000000000000000000000000002")
    
    const owner = accounts [0] || process.env.OWNER;
    const escrow =  accounts[9] || process.env.ESCROW;

    it ("1. deploy & mint ERC721", async () => { 
        mpKSM = await MarketPlace.deployed();

        t721 =  await Token721.new( "CryptoPunk1", "CP1");
        await t721.mint(owner, idNFT);
       assert (owner == await t721.ownerOf(idNFT), "owner !=  t721.ownerOf(idNFT)");
       
    }),

    it ("1.1 configure escrow", async () => { 

        const isEscrow = await mpKSM.setEscrow(escrow, true);
       
    }),
    it ("2. make ask", async () => { 

        await t721.approve(mpKSM.address, idNFT, {from: owner});
        await  mpKSM.addAsk (  price, //price
            addrKSM, //_currencyCode
//            0x8d268ccc5844851e91bed6fe63457d1202c20719
              //0x0000000000000000000000000000000000000002
                        t721.address, //_idCollection
                        idNFT, 
                        {from: owner}
                    );

       assert (mpKSM.address == await t721.ownerOf(idNFT), "mpKSM.address !=  t721.ownerOf(idNFT)");
        const askID = await mpKSM.asks(t721.address, idNFT);
        const order = await mpKSM.orders(askID);
       assert (order.idNFT.toNumber() ==  idNFT, "order.idNFT. !=  idNFT");
    }),
    it ("3. make deposit", async () => { 

        await  mpKSM.depositKSM (  depoSum, //
                         //_currencyCode
                        accounts[1], //sender
                        {from: escrow} );
        const balanceKSM = await mpKSM.balanceKSM(accounts[1])
   //     console.log ("balanceKSM", balanceKSM.toNumber());
       assert (depoSum ==  balanceKSM.toNumber(), "depoSum !=  balanceKSM", depoSum,   balanceKSM.toNumber());

    }),

    it ("4. buying", async () => { 

      await  mpKSM.buyKSM (t721.address, //_idCollection
                         idNFT, accounts[1], accounts[2],  {from:  accounts[1]});
    
      const balanceKSM = await mpKSM.balanceKSM(accounts[1])
       assert (depoSum - price == balanceKSM.toNumber(), "depoSum - price !=  balanceKSM", depoSum - price, balanceKSM.toNumber());
      const askID = await mpKSM.asks( t721.address, idNFT);
      const order = await mpKSM.orders(askID);
       assert (order.flagActive.toNumber() == 0, "order.flagActive != 0");
       const newOwner =  await t721.ownerOf(idNFT);
       assert (accounts[2] == newOwner, "accounts2 != t721.ownerOf(idNFT)", accounts[2],  newOwner );


    }) //, 

     it ("5. withdrawing", async () => { 

        await  mpKSM.withdrawAllKSM (owner);
      
        const balanceKSM = await mpKSM.balanceKSM(owner)
         assert ( balanceKSM.toNumber() == 0, " balanceKSM != 0");
  
  
      }),

      it ("6 deconfigure escrow", async () => { 

        const isEscrow = await mpKSM.setEscrow(escrow, false);
       
    })
 
})