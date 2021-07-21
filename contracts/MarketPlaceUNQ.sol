// SPDX-License-Identifier:  Apache License
pragma solidity >= 0.8.0;
import "@openzeppelin/contracts/token/ERC721/IERC721.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/token/ERC721/IERC721Receiver.sol";
import "@openzeppelin/contracts/utils/math/SafeMath.sol";


contract MarketPlaceUNQ is IERC721Receiver {
    using SafeMath for uint;
    struct Offer {
        
        uint256 idNFT;
        address currencyCode; //address of currency  token, = address(0) for UNQ
        uint256 price;
        uint256 time;
        address idCollection;
        address userAddr;
        uint8 flagActive;        
    }
    Offer[] public  offers;

    mapping (address => mapping (uint256 => uint256)) public balanceKSM;  //  [userAddr] => [KSMs]
    mapping (address => mapping (address => mapping (uint256 => uint256))) public  asks ; // [buyer][idCollection][idNFT] => idOffer

    mapping (address => uint[]) public asksbySeller; // [addressSeller] =>idOffer

    address escrow;
    address owner;

    constructor (address _escrow) {
        escrow = _escrow;
        owner = msg.sender;

    }

   function setowner  (address _newEscrow) public onlyOwner {
        escrow = _newEscrow;
    }

    function setEscrow  (address _newEscrow) public onlyOwner {
        escrow = _newEscrow;
    }

    modifier onlyEscrow () {
        require(msg.sender == escrow, "Only escrow can");
        _;
    }

    modifier onlyOwner () {
        require(msg.sender == owner, "Only owner can");
        _;
    }

    /**
    * Make bids (offers) to sell NFTs 
    */
    function setAsk (uint256 _price, 
                    address  _currencyCode, 
                    address _idCollection, 
                    uint256 _idNFT,
                    uint8 _active ) public  { //
        
        require (IERC721(_idCollection).ownerOf(_idNFT) == msg.sender, "Not right token owner");
        uint offerID =  asks[msg.sender][_idCollection][_idNFT];
        if (offers.length == 0 || offers[offerID].idCollection == address(0)){
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




    function buy (address _idCollection, uint256 _idNFT ) public payable { //buing for UNQ like as ethers 
        
        Offer memory offer = offers[ asks[msg.sender][_idCollection][_idNFT]];
        //1. check sent amount and send to seller
        require (msg.value == offer.price, "Not right amount UNQ sent, have to be equal price" );     
        payable(offer.userAddr).transfer(offer.price); 
        // 2. close offer
        offers[ asks[msg.sender][_idCollection][_idNFT]].flagActive = 0;
        // 3. transfer NFT to buyer
        IERC721(_idCollection).transferFrom(address(this), msg.sender, _idNFT);


    }


    function buy (address _idCollection, uint256 _idNFT, address _currencyCode, uint _amount ) public payable {
        
        Offer memory offer = offers[ asks[msg.sender][_idCollection][_idNFT]];
        //1. check sent amount and transfer from buyer to seller
        require (offer.price == _amount && offer.currencyCode == _currencyCode, "Not right amount or currency sent, have to be equal currency and price" );
        // !!! transfer have to be approved to marketplace!
        IERC20(offer.currencyCode).transferFrom(msg.sender, address(this), offer.price); //to not disclojure buyer's address 
        IERC20(offer.currencyCode).transfer(offer.userAddr, offer.price);
        // 2. close offer
        offers[ asks[msg.sender][_idCollection][_idNFT]].flagActive = 0;
        // 3. transfer NFT to buyer
        IERC721(_idCollection).transferFrom(address(this), msg.sender, _idNFT);


    }


    function onERC721Received(address operator, address from, uint256 tokenId, bytes calldata data)  public override pure returns(bytes4) {
            return bytes4(keccak256("onERC721Received(address,address,uint256,bytes)"));
        }
}