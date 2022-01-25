const { assert } = require("console");
require('dotenv').config();
const owner =  process.env.OWNER;
const escrow =  process.env.ESCROW;
const helper = process.env.HELPERS;
const seller = process.env.TEST_SELLER;
const buyer =  process.env.TEST_BUYER;

const MarketPlace = artifacts.require('MarketPlace.sol');
const ContractHelper = artifacts.require('interfaces/ContractHelpers.sol')

const Token721 = artifacts.require('ERC721example.sol');




contract ("MarketPlace for KSM", accounts => {
    var mpKSM, t721;
    const idNFT = 1234;
    const price = 1000;
    const depoSum = 1500;
    const addrKSM = web3.utils.toChecksumAddress("0x0000000000000000000000000000000000000002")
    
    const owner =  process.env.OWNER;
    const escrow =  process.env.ESCROW;
    
    


    it ("0. configure market, escrow", async () => { 
        const networkId = await web3.eth.net.getId();  
        const adrs = require('../addresses.json')
        mpKSM = await MarketPlace.at(adrs[networkId].marketplace);

        const isEscrow = await mpKSM.setEscrow(escrow, true);
        assert (isEscrow, "Escrow not changed");

        
        if (networkId == "8888") {        
            
            const ch = await  ContractHelper.at(helper);
            assert (await ch.sponsoringEnabled(mpKSM.address), "Sponsoring not enabled" )
            await ch.toggleAllowed(mpKSM.address, seller, true);
            await ch.toggleAllowed(mpKSM.address, buyer, true);
        } else {
            console.error("test only for Opal chain");
            
        }      
    }),
    it ("1. deploy & mint ERC721", async () => { 

        t721 =  await Token721.new( "CryptoPunk1", "CP1");
        await t721.mint(seller, idNFT);
       assert (seller == await t721.ownerOf(idNFT), "owner !=  t721.ownerOf(idNFT)");
       
    }),

    it ("2. make ask", async () => { 

        await t721.approve(mpKSM.address, idNFT, {from: seller});
        await  mpKSM.addAsk (  price, //price
            addrKSM, //_currencyCode
//            0x8d268ccc5844851e91bed6fe63457d1202c20719
              //0x0000000000000000000000000000000000000002
                        t721.address, //_idCollection
                        idNFT, 
                        {from: seller}
                    );

       assert (mpKSM.address == await t721.ownerOf(idNFT), "mpKSM.address !=  t721.ownerOf(idNFT)");
        const askID = await mpKSM.asks(t721.address, idNFT);
        const order = await mpKSM.orders(askID);
       assert (order.idNFT.toNumber() ==  idNFT, "order.idNFT. !=  idNFT");
    }),

    it ("3. make deposit", async () => { 

        await  mpKSM.depositKSM (  depoSum, //
                         //_currencyCode
                        buyer, //sender
                        {from: escrow} );
        const balanceKSM = await mpKSM.balanceKSM(buyer)
   //     console.log ("balanceKSM", balanceKSM.toNumber());
       assert (depoSum ==  balanceKSM.toNumber(), "depoSum !=  balanceKSM", depoSum,   balanceKSM.toNumber());

    }),

    it ("4. buying", async () => { 

      await  mpKSM.buyKSM (t721.address, //_idCollection
                         idNFT, buyer, buyer,  {from:  buyer});
    
      const balanceKSM = await mpKSM.balanceKSM(buyer)
       assert (depoSum - price == balanceKSM.toNumber(), "depoSum - price !=  balanceKSM", depoSum - price, balanceKSM.toNumber());
      const askID = await mpKSM.asks( t721.address, idNFT);
      const order = await mpKSM.orders(askID);
       assert (order.flagActive.toNumber() == 0, "order.flagActive != 0");
       const newOwner =  await t721.ownerOf(idNFT);
       assert (buyer == newOwner, "accounts2 != t721.ownerOf(idNFT)", buyer,  newOwner );


    }) //, 

     it ("5. withdrawing", async () => { 

        await  mpKSM.withdrawAllKSM (seller,  {from:seller});
      
        const balanceKSM = await mpKSM.balanceKSM(seller)
         assert ( balanceKSM.toNumber() == 0, "seller's balanceKSM != 0");
  
  
      }),

      it ("6 deconfigure escrow", async () => { 

        const isEscrow = await mpKSM.setEscrow(escrow, false);
       
    })
 
})