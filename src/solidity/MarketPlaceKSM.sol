// SPDX-License-Identifier:  Apache License
pragma solidity >= 0.8.0;


contract MarketPlaceKSM {
struct Offer {
    uint idCollection;
    uint idNFT;
    uint currencyCode;
    uint price;
    address useraaDDR;
    bool flagActive;
}
//Offer  offers[];

mapping (address => uint256) balanceNFt;  //  [useraaDDR] => [nftID]
mapping (uint256 => mapping (uint => Offer))   asks ; // [idCollection][idNFT] = offer




function ask (uint _price, uint  _currencyCode, uint _palet, uint _tokenID ) public payable { //


}

function deposit () public payable {


}

function buy (uint _palet, uint _tokenID ) public {

}

function withdraw (uint _amoint ) public {


}

}