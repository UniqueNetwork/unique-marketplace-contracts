// SPDX-License-Identifier:  Apache License
pragma solidity >= 0.8.0;
import "./openzeppelincontracts/token/ERC721/IERC721.sol";
import "./openzeppelincontracts/token/ERC721/IERC721Receiver.sol";
import "./openzeppelincontracts/utils/math/SafeMath.sol";


contract MarketPlaceKSM is IERC721Receiver {
    using SafeMath for uint;
    struct Offer {
        
        uint256 idNFT;
        uint256 currencyCode;
        uint256  price;
        uint256 time;
        address idCollection;
        address useraaDDR;
        uint8 flagActive;        
    }
    Offer[] public  offers;

    mapping (address => uint256) balanceNFt;  //  [useraaDDR] => [nftID]
    mapping (address => mapping (uint256 => uint256)) balanceKSM;  //  [useraaDDR] => [KSMs]
    mapping (address => mapping (address => mapping (uint256 => uint256)))   asks ; // [buyer][idCollection][idNFT] => idOffer

    mapping (address => uint[]) asksbySeller; // [addressSeller] =>idOffer

    address escrow;

    constructor (address _escrow) {
        escrow = _escrow;

    }

    function setEscrow  (address _newEscrow) public onlyEscrow {
        escrow = _newEscrow;
    }

    modifier onlyEscrow () {
        require(msg.sender == escrow, "Only escrow can");
        _;
    }

    /**
    * Make bids (offers) to sell NFTs 
    */
    function setAsk (uint256 _price, 
                    uint256  _currencyCode, 
                    address _idCollection, 
                    uint256 _idNFT,
                    uint8 _active ) public  { //
        
        require (IERC721(_idCollection).ownerOf(_idNFT) == msg.sender, "Not right token owner");

        if (offers[asks[msg.sender][_idCollection][_idNFT]].flagActive == 0){
            offers.push(Offer(        
                    _idNFT,
                    _currencyCode,
                    _price,
                    block.timestamp,
                    _idCollection,
                    msg.sender,
                    _active
                ));
            asks[msg.sender][_idCollection][_idNFT] = offers.length-1;
            asksbySeller[msg.sender].push(offers.length-1);
            } else //edit existing offer
            {
                offers[asks[msg.sender][_idCollection][_idNFT]] = Offer(        
                    offers[asks[msg.sender][_idCollection][_idNFT]].idNFT,
                    _currencyCode,
                    _price,
                    block.timestamp,
                    offers[asks[msg.sender][_idCollection][_idNFT]].idCollection,
                    msg.sender,
                    _active);
            }

            IERC721(_idCollection).transferFrom(msg.sender, address(this), _idNFT);

    }


    function deposit (uint256 _amount, uint256 _currencyCode, address _sender   ) public onlyEscrow {

        balanceKSM[_sender][_currencyCode].add(_amount);

    }

    function buy (address _idCollection, uint256 _idNFT ) public {
        
        Offer memory offer = offers[ asks[msg.sender][_idCollection][_idNFT]];
        //1. reduce balance
        balanceKSM[msg.sender][offer.currencyCode].sub( offer.price, "Insuccificient KSMs funds");
        // 2. close offer
        offers[ asks[msg.sender][_idCollection][_idNFT]].flagActive = 0;
        // 3. transfer NFT to buyer
        IERC721(_idCollection).transferFrom(address(this), msg.sender, _idNFT);

    }

    function withdraw (uint256 _amount, uint256 _currencyCode, address _sender   ) public  onlyEscrow returns (bool ){
        balanceKSM[_sender][_currencyCode].sub( _amount, "Insuccificient KSMs balance");
        return true;


    }

    function onERC721Received(address operator, address from, uint256 tokenId, bytes calldata data)  public override pure returns(bytes4) {
            return bytes4(keccak256("onERC721Received(address,address,uint256,bytes)"));
        }
}