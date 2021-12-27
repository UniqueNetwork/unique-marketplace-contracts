
//const BridgeGate = artifacts.require('BridgeGate.sol');
const MarketPlaceKSM = artifacts.require('MarketPlace.sol');
const { deployProxy } = require('@openzeppelin/truffle-upgrades');


//const ERC721example = artifacts.require('ERC721example.sol');


module.exports = async function(deployer,_network, addresses) {
   
      const networkId = await web3.eth.net.getId();     
      //await deployer.deploy(MarketPlaceKSM, addresses[0]);
      // const mp = await MarketPlaceKSM.deployed();
      // upgradable deploys
      const mp = await deployProxy(MarketPlaceKSM, addresses[0], addresses[0], { deployer });
      console.log('Deployed upgradable: ', mp.address);
      
      await mp.setNativeCoin(web3.utils.toChecksumAddress("0x0000000000000000000000000000000000000001"));
      var addresses = require ("../addresses.json");

      // console.log ("MarketPlace:",  mp.address)
      addresses[networkId].marketplace = mp.address;
      addresses[networkId].account = addresses[0];
      let fs = require('fs');
      fs.writeFileSync("./addresses.json", JSON.stringify(addresses), function(err) {
            if (err) {
                console.log(err);
            }
      });
   // upgrade branch

   /**
    * // migrations/MM_upgrade_box_contract.js
      const { upgradeProxy } = require('@openzeppelin/truffle-upgrades');

      const Box = artifacts.require('Box');
      const BoxV2 = artifacts.require('BoxV2');

      module.exports = async function (deployer) {
      const existing = await Box.deployed();
      const instance = await upgradeProxy(existing.address, BoxV2, { deployer });
      console.log("Upgraded", instance.address);
};
    */
};
