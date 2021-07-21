
//const BridgeGate = artifacts.require('BridgeGate.sol');
const MarketPlaceUNQ = artifacts.require('MarketPlaceUNQ.sol');


//const ERC721example = artifacts.require('ERC721example.sol');


module.exports = async function(deployer,_network, addresses) {
   

      await deployer.deploy(MarketPlaceUNQ, addresses[0]);
      const mpKSM = await MarketPlaceUNQ.deployed();
      console.log ("MarketPlaceUNQ:",  mpKSM.address)

  
};
