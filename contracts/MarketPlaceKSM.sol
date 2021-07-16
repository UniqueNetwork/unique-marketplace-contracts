// SPDX-License-Identifier:  Apache License
pragma solidity >= 0.8.0;
import "./openzeppelincontracts/token/ERC721/IERC721.sol";
import "./openzeppelincontracts/token/ERC721/IERC721Receiver.sol";


contract MarketPlaceKSM is IERC721Receiver {
struct Offer {
    
    uint256 idNFT;
    uint256 currencyCode;
    uint256  price;
    address idCollection;
    address useraaDDR;
    uint8 flagActive;
}
Offer[] public  offers;

mapping (address => uint256) balanceNFt;  //  [useraaDDR] => [nftID]
mapping (address => mapping (uint256 => uint))   asks ; // [idCollection][idNFT] => idOffer
mapping (address => uint[]) asksbySeller; // [addressSeller] =>idOffer


/**
* Make bids (offers) to sell NFTs 
 */
function setAsk (uint256 _price, 
                uint256  _currencyCode, 
                address _idCollection, 
                uint256 _idNFT,
                uint8 _active ) public payable { //
    
    require (IERC721(_idCollection).ownerOf(_idNFT) == msg.sender, "Not right token owner");

    if (offers[asks[_idCollection][_idNFT]].idCollection == address(0)){
        offers.push(Offer(        
                _idNFT,
                _currencyCode,
                _price,
                _idCollection,
                msg.sender,
                _active
            ));
        asks[_idCollection][_idNFT] = offers.length-1;
        asksbySeller[msg.sender].push(offers.length-1);
        } else //edit existing offer
        {
            offers[asks[_idCollection][_idNFT]] = Offer(        
                _idNFT,
                _currencyCode,
                _price,
                _idCollection,
                msg.sender,
                _active);
        }


}


function deposit () public payable {


}

function buy (uint256 _palet, uint256 _tokenID ) public {

}

function withdraw (uint256 _amoint ) public {


}

function onERC721Received(address operator, address from, uint256 tokenId, bytes calldata data)  public override pure returns(bytes4) {
        return bytes4(keccak256("onERC721Received(address,address,uint256,bytes)"));
    }
}