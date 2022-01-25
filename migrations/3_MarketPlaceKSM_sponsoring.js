
require('dotenv').config();
const helper = process.env.HELPERS;
const owner =  process.env.OWNER;
const amountToSend =  process.env.AMOUNT || 100;

const ContractHelper = artifacts.require('interfaces/ContractHelpers.sol');
const MarketPlaceKSM = artifacts.require('MarketPlace.sol');

var mp;

module.exports = async function(deployer,_network, addresses) {
  const networkId = await web3.eth.net.getId();  
  if (networkId == "8888")  {

       const mp = await MarketPlaceKSM.deployed();
       const ch = await  ContractHelper.at(helper);
      //  console.log ('toggleAllowlist',  await ch.toggleAllowlist(mp.address, false, {from:owner}));
      var tx;
      
      console.log ('switch off sponsoring before send funds' );
      tx = await ch.toggleSponsoring(mp.address, false, {from:owner})
      console.log (tx.receipt ); 
      var balance = await web3.eth.getBalance( mp.address); //Will give value in.

      console.log ("balance MP before",  web3.utils.fromWei(balance)   ); 
       console.log ("send funds" ); 
      const amountToSendW =  web3.utils.toWei (amountToSend)
      tx = await web3.eth.sendTransaction({ from: owner, to: mp.address, value: amountToSendW }) 
       balance = await  web3.eth.getBalance( mp.address); //Will give value in.
      
      console.log ("balance MP after", web3.utils.fromWei(balance)  ); 
      console.log ('toggle Sponsoring' );
      tx = await ch.toggleSponsoring(mp.address, true, {from:owner})
      console.log (tx.receipt ); 
      console.log ("set SponsoringRateLimit" ); 
      tx = await ch.setSponsoringRateLimit(mp.address, 1, {from:owner})
      console.log (tx.receipt ); 
      
      
       
       
       /* 
        var addresses = require ("../addresses.json");
      addresses[networkId].marketplace = mp.address;
      addresses[networkId].account = owner;
      let fs = require('fs');
      fs.writeFileSync("./addresses.json", JSON.stringify(addresses), function(err) {
            if (err) {
                console.log(err);
            }
      }); */
  } 

};
