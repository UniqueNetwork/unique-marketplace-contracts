
//const BridgeGate = artifacts.require('BridgeGate.sol');
const MarketPlaceKSM = artifacts.require('MarketPlace.sol');


//const ERC721example = artifacts.require('ERC721example.sol');


module.exports = async function(deployer,_network, addresses) {
   

      await deployer.deploy(MarketPlaceKSM, addresses[0]);
      const mp = await MarketPlaceKSM.deployed();
      await mp.setNativeCoin(web3.utils.toChecksumAddress("0x0000000000000000000000000000000000000001"));
      // console.log ("MarketPlace:",  mp.address)

  
};
