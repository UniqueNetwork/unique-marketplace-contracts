const { assert } = require("console");


const MarketPlace = artifacts.require('MarketPlace.sol');

const Token721 = artifacts.require('ERC721example.sol');



contract ("MarketPlace", accounts => {
    var mpKSM, t721;
    const idNFT = 1234;
    const price = 1000;
    const depoSum = 1500;
    const addrKSM = web3.utils.toChecksumAddress("0x0000000000000000000000000000000000000002")
    
    it ("1. deploy & mint ERC721", async () => { 
        mpKSM = await MarketPlace.deployed();

        t721 =  await Token721.new( "CryptoPunk1", "CP1");
        await t721.mint(accounts[0], idNFT);
       assert (accounts[0] == await t721.ownerOf(idNFT), "accounts[0] !=  t721.ownerOf(idNFT)");
       
    }),
    it ("2. make ask bid", async () => { 

        await t721.approve(mpKSM.address, idNFT);
        await  mpKSM.addAsk (  price, //price
            addrKSM, //_currencyCode
//            0x8d268ccc5844851e91bed6fe63457d1202c20719
              //0x0000000000000000000000000000000000000002
                        t721.address, //_idCollection
                        idNFT,

        );

       assert (mpKSM.address == await t721.ownerOf(idNFT), "mpKSM.address !=  t721.ownerOf(idNFT)");
        const askID = await mpKSM.asks(t721.address, idNFT);
        const order = await mpKSM.orders(askID);
       assert (order.idNFT.toNumber() ==  idNFT, "order.idNFT. !=  idNFT");
    }),
    it ("3. make deposit", async () => { 

        await  mpKSM.deposit (  depoSum, //
            addrKSM, //_currencyCode
                        accounts[0], //sender
                         );
        const balanceKSM = await mpKSM.balanceKSM(accounts[0],  addrKSM,)
   //     console.log ("balanceKSM", balanceKSM.toNumber());
       assert (depoSum ==  balanceKSM.toNumber(), "depoSum !=  balanceKSM");

    }),

    it ("4. buying", async () => { 

      await  mpKSM.buyKSM (t721.address, //_idCollection
                         idNFT);
    
      const balanceKSM = await mpKSM.balanceKSM(accounts[0],  addrKSM,)
       assert (depoSum - price == balanceKSM.toNumber(), "depoSum - price !=  balanceKSM");
      const askID = await mpKSM.asks( t721.address, idNFT);
      const order = await mpKSM.orders(askID);
       assert (order.flagActive.toNumber() == 0, "order.flagActive != 0");
       assert (accounts[0] == await t721.ownerOf(idNFT), "accounts[0] != t721.ownerOf(idNFT)");


    }) //, 

     it ("5. withdrawing", async () => { 

        await  mpKSM.withdrawKSM (depoSum-price, //_idCollection
            addrKSM, //_currenceCode
                                accounts[0]);
      
        const balanceKSM = await mpKSM.balanceKSM(accounts[0], addrKSM)
         assert ( balanceKSM.toNumber() == 0, " balanceKSM != 0");
  
  
      })
 
})