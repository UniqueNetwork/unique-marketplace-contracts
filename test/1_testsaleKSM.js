const { assert } = require("console");


const MarketPlaceKSM = artifacts.require('MarketPlaceKSM.sol');

const Token721 = artifacts.require('ERC721example.sol');



contract ("MarketPlaceKSM", accounts => {
    var mpKSM, t721;
    const idNFT = 1234;
    const price = 1000;
    const depoSum = 1500;
    it ("1. deploy & mint ERC721", async () => { 
        mpKSM = await MarketPlaceKSM.deployed();

        t721 =  await Token721.new( "CryptoPunk1", "CP1");
        await t721.mint(accounts[0], idNFT);
       assert (accounts[0] == await t721.ownerOf(idNFT), "accounts[0] !=  t721.ownerOf(idNFT)");
       
    }),
    it ("2. make ask bid", async () => { 

        await t721.approve(mpKSM.address, idNFT);
        await  mpKSM.setAsk (  price, //price
                        1, //_currencyCode
                        t721.address, //_idCollection
                        idNFT,
                        1 //isactive
        );

       assert (mpKSM.address == await t721.ownerOf(idNFT), "mpKSM.address !=  t721.ownerOf(idNFT)");
        const askID = await mpKSM.asks(accounts[0], t721.address, idNFT);
        const order = await mpKSM.orders(askID);
       assert (order.idNFT.toNumber() ==  idNFT, "order.idNFT. !=  idNFT");
    }),
    it ("3. make deposit", async () => { 

        await  mpKSM.deposit (  depoSum, //
                        1, //_currencyCode
                        accounts[0], //sender
                         );
        const balanceKSM = await mpKSM.balanceKSM(accounts[0], 1)
   //     console.log ("balanceKSM", balanceKSM.toNumber());
       assert (depoSum ==  balanceKSM.toNumber(), "depoSum !=  balanceKSM");

    }),

    it ("4. buying", async () => { 

      await  mpKSM.buy (t721.address, //_idCollection
                         idNFT);
    
      const balanceKSM = await mpKSM.balanceKSM(accounts[0], 1)
       assert (depoSum - price == balanceKSM.toNumber(), "depoSum - price !=  balanceKSM");
      const askID = await mpKSM.asks(accounts[0], t721.address, idNFT);
      const order = await mpKSM.orders(askID);
       assert (order.flagActive.toNumber() == 0, "order.flagActive != 0");
       assert (accounts[0] == await t721.ownerOf(idNFT), "accounts[0] != t721.ownerOf(idNFT)");


    }), 

    it ("5. withdrawing", async () => { 

        await  mpKSM.withdraw (depoSum-price, //_idCollection
                                1,
                                accounts[0]);
      
        const balanceKSM = await mpKSM.balanceKSM(accounts[0], 1)
         assert ( balanceKSM.toNumber() == 0, " balanceKSM != 0");
  
  
      })

})