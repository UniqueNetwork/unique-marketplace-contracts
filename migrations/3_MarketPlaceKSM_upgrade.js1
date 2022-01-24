
const upgradeFlag= false; // change to true when upgrading
const MarketPlaceKSM = artifacts.require('MarketPlace.sol');

const { deployProxy } = require('@openzeppelin/truffle-upgrades');
const { upgradeProxy } = require('@openzeppelin/truffle-upgrades');

var mp;

module.exports = async function(deployer,_network, addresses) {
  if (upgradeFlag)  {
      const MarketPlaceKSMnew = artifacts.require('MarketPlace_new.sol');
      const networkId = await web3.eth.net.getId();     
      //await deployer.deploy(MarketPlaceKSM, addresses[0]);
      // const mp = await MarketPlaceKSM.deployed();
      // upgradable deploys
      
      // upgrade branch https://forum.openzeppelin.com/t/openzeppelin-upgrades-step-by-step-tutorial-for-truffle/3579
           // docs: https://docs.openzeppelin.com/upgrades-plugins/1.x/  
        
        const existing = await MarketPlaceKSM.deployed();
        mp = await upgradeProxy(existing.address, MarketPlaceKSMnew, { deployer });
        console.log("Upgraded", mp.address);
    
      // console.log ("MarketPlace:",  mp.address)
      await mp.setNativeCoin(web3.utils.toChecksumAddress("0x0000000000000000000000000000000000000001"));
        var addresses = require ("../addresses.json");
      addresses[networkId].marketplace = mp.address;
      addresses[networkId].account = addresses[0];
      let fs = require('fs');
      fs.writeFileSync("./addresses.json", JSON.stringify(addresses), function(err) {
            if (err) {
                console.log(err);
            }
      });
  }
};
