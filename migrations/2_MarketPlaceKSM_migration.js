require('dotenv').config();
const owner =  process.env.OWNER;
const escrow =  process.env.ESCROW;

const upgradeFlag= false; // change to true when upgrading

//const BridgeGate = artifacts.require('BridgeGate.sol');
const MarketPlaceKSM = artifacts.require('MarketPlace.sol');

const { deployProxy } = require('@openzeppelin/truffle-upgrades');
const { upgradeProxy } = require('@openzeppelin/truffle-upgrades');


var mp;
//const ERC721example = artifacts.require('ERC721example.sol');

module.exports = async function(deployer,_network, addresses) {
      const networkId = await web3.eth.net.getId(); 
      if (upgradeFlag)  {
          const MarketPlaceKSMnew = artifacts.require('MarketPlace_new.sol');
        
        // upgradable deploys
        
        // upgrade branch https://forum.openzeppelin.com/t/openzeppelin-upgrades-step-by-step-tutorial-for-truffle/3579
             // docs: https://docs.openzeppelin.com/upgrades-plugins/1.x/  
          
          const existing = await MarketPlaceKSM.deployed();
          mp = await upgradeProxy(existing.address, MarketPlaceKSMnew, { deployer });
          console.log("Upgraded", mp.address);
      
    }
    else  {    
     // await deployer.deploy(MarketPlaceKSM, addresses[0], addresses[0]);
      // mp = await MarketPlaceKSM.deployed();
    //  console.log ("MarketPlace:",  mp.address)
       
      // upgradable deploys
     mp = await deployProxy(MarketPlaceKSM, { deployer });
      console.log('Deployed upgradable: ', mp.address);
    }
      await mp.setEscrow(escrow, true, {from: owner});
      await mp.setNativeCoin(web3.utils.toChecksumAddress("0x0000000000000000000000000000000000000001"), {from: owner});
        var addresses = require ("../addresses.json");
      addresses[networkId].marketplace = mp.address;
      addresses[networkId].owner = owner;
      addresses[networkId].escrow = escrow;
      let fs = require('fs');
      fs.writeFileSync("./addresses.json", JSON.stringify(addresses), function(err) {
            if (err) {
                console.log(err);
            }
      });
};
