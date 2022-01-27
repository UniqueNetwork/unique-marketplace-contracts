const { assert } = require("console");
require('dotenv').config();
const owner =  process.env.OWNER;
const escrow =  process.env.ESCROW;
const helper = process.env.HELPERS;
const seller = process.env.TEST_SELLER;
const buyer =  process.env.TEST_BUYER;
const amountToSend =  process.env.AMOUNT || 500;

const MarketPlace = artifacts.require('MarketPlace.sol');
const ContractHelper = artifacts.require('interfaces/ContractHelpers.sol')

const Token721 = artifacts.require('ERC721example.sol');

contract ("MarketPlace for KSM with sponsoring", accounts => {
    var mpKSM, t721, tx, ch;
    const idNFT = 1234;
    const price = 1000;
    const depoSum = 1500;
    const addrKSM = web3.utils.toChecksumAddress("0x0000000000000000000000000000000000000002")

    it ("0. configure market, escrow", async () => { 
        const networkId = await web3.eth.net.getId();  
        const adrs = require('../addresses.json')
        mpKSM = await MarketPlace.at(adrs[networkId].marketplace);
        balance = await  web3.eth.getBalance( mpKSM.address); //Will give value in.
 
        console.log ("balance mpKSM.address before", web3.utils.fromWei(balance)  ); 
        const isEscrow = await mpKSM.setEscrow(escrow, true);
        assert (isEscrow, "Escrow not changed")

        
        if (networkId == "8888") {  
      
           /*  const implMPAddr = '0xcf2ac9a59bd292C4634B41ceEd93C2307B45812A'
            balance = await  web3.eth.getBalance( implMPAddr); //Will give value in.
     
            console.log ("balance implMPAddr before", web3.utils.fromWei(balance)  ); 
     
            const admMPAddr = '0xA7a7d5Caa966Ee4cd76D95A426f5E9334eBb753B'
            balance = await  web3.eth.getBalance( admMPAddr); //Will give value in.
     
            console.log ("balance admMPAddr before", web3.utils.fromWei(balance)  ); 
      */
          
            
            ch = await  ContractHelper.at(helper);

            assert (await ch.sponsoringEnabled(mpKSM.address), "Sponsoring not enabled" )
            await ch.toggleAllowed(mpKSM.address, seller, true);
            await ch.toggleAllowed(mpKSM.address, buyer, true);
            assert (await ch.allowed(mpKSM.address, buyer), "buyer not sponsored");
            assert (await ch.allowed(mpKSM.address, seller), "seller not sponsored");
        } else {
            console.error("wrong chain. Test shall works correctly only for Opal chain");
            
        }      
    }),
    it ("1. deploy & mint ERC721", async () => { 

        t721 =  await Token721.new( "CryptoPunk1", "CP1" , {from: owner} );
        //////// add sponsorship for 
        var balance = await web3.eth.getBalance( t721.address); //Will give value in.

        console.log ("balance t721.address before",  web3.utils.fromWei(balance)   ); 
        console.log ("send funds", amountToSend ); 
        const amountToSendW =  web3.utils.toWei (amountToSend)

        tx = await web3.eth.sendTransaction({ from: owner, to: t721.address, value: amountToSendW }) 
        balance = await  web3.eth.getBalance( t721.address); //Will give value in.
        
        console.log ("balance t721.address after", web3.utils.fromWei(balance)  ); 
        console.log ('toggle Sponsoring' );
        tx = await ch.toggleSponsoring(t721.address, true, {from:owner})
        // console.log (tx.receipt ); 
        console.log ("set SponsoringRateLimit" ); 
        tx = await ch.setSponsoringRateLimit(t721.address, 0, {from:owner})
        // console.log (tx.receipt ); 
        await ch.toggleAllowed(t721.address, seller, true);
        await ch.toggleAllowed(t721.address, buyer, true);
        assert (await ch.allowed(t721.address, buyer), "buyer not sponsored");
        assert (await ch.allowed(t721.address, seller), "seller not sponsored");
        
        await t721.mint(seller, idNFT, {from: seller});
        assert (seller == await t721.ownerOf(idNFT), "owner !=  t721.ownerOf(idNFT)");
       
    }),

    it ("2. make ask", async () => { 

        await t721.approve(mpKSM.address, idNFT, {from: seller} );
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
        balance = await  web3.eth.getBalance( t721.address); //Will give value in.
        console.log ("balance t721.address after", web3.utils.fromWei(balance)  );
        balance = await  web3.eth.getBalance( mpKSM.address); //Will give value in.
 
        console.log ("balance mpKSM.address after", web3.utils.fromWei(balance)  ); 
        /* const implMPAddr = '0xcf2ac9a59bd292C4634B41ceEd93C2307B45812A'
        balance = await  web3.eth.getBalance( implMPAddr); //Will give value in.
 
        console.log ("balance implMPAddr after", web3.utils.fromWei(balance)  ); 
 
        const admMPAddr = '0xA7a7d5Caa966Ee4cd76D95A426f5E9334eBb753B'
        balance = await  web3.eth.getBalance( admMPAddr); //Will give value in.
 
        console.log ("balance admMPAddr after", web3.utils.fromWei(balance)  );  */
 


    })
 
})