const { assert } = require("console");


const MarketPlace = artifacts.require('MarketPlace.sol');

const Token721 = artifacts.require('ERC721example.sol');
//const TokenERC20 = artifacts.require('TokTst.sol');
const erc = web3.utils.toChecksumAddress("0x0000000000000000000000000000000000000001")

contract ("MarketPlace for UNQ", accounts => {
    var mp, t721;
    const idNFT = 1234;
    const price = 1000 ;
    const price2 =  2000;

    it ("1. deploy & mint ERC721, ERC20 ", async () => { 

        t721 =  await Token721.new( "CryptoPunk1", "CP1");
        await t721.mint(accounts[0], idNFT);

       assert (accounts[0] == await t721.ownerOf(idNFT), "accounts[0] !=  t721.ownerOf(idNFT)");
       
    }),
    it ("2. make ask bid", async () => { 
        mp = await MarketPlace.deployed();

        await t721.approve(mp.address, idNFT);
        await  mp.addAsk (  price, //price
               erc, //_currencyCode
               t721.address, //_idCollection
               idNFT,
                {from: accounts[0], /* gasPrice: "0x01" */}
                );

       assert (mp.address == await t721.ownerOf(idNFT), "mp.address !=  t721.ownerOf(idNFT)");
        const askID = await mp.asks(t721.address, idNFT);
        const order = await mp.orders(askID);
       assert (order.idNFT.toNumber() ==  idNFT, "order.idNFT. !=  idNFT");
    }),
    it ("3.change order", async () => { 

        await  mp.editAsk ( price2, //
                erc, //_currencyCode
                t721.address,
                idNFT,
                1,
                {from: accounts[0], /* gasPrice: "0x01" */}  );
        const askID = await mp.asks(t721.address, idNFT);
        const order = await mp.orders(askID);
       assert (order.price  ==  price2, "price didn't changed");

    }),

    it ("4. buying", async () => { 
      const tx= await  mp.buy  (t721.address, //_idCollection
                                idNFT,
                                { from: accounts[0], 
                               /*  gasPrice: "0x01", */ 
                                value: price2 }  );
      const askID = await mp.asks( t721.address, idNFT);
      const order = await mp.orders(askID);
    //   console.log (order.flagActive.toNumber(), await t721.ownerOf(idNFT), tx.tx);
       assert (order.flagActive == 0, "order.flagActive != 0");
       assert (accounts[0] == await t721.ownerOf(idNFT), "accounts[0] != t721.ownerOf(idNFT)");


    }), 
    it ("5. withdrawing", async () => { 

        await  mp.withdraw (price2, 
                erc, //_currencyCode
               {from: accounts[0]} );
      
        const balanceKSM = await mp.balanceKSM(accounts[0])
         assert ( balanceKSM.toNumber() == 0, " balanceKSM != 0");
  
  
      })
 
})