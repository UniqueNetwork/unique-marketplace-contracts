// SPDX-License-Identifier:  Apache License
pragma solidity >= 0.8.0;
import "@openzeppelin/contracts/token/ERC721/IERC721.sol";
import "@openzeppelin/contracts/token/ERC721/IERC721Receiver.sol";
import "@openzeppelin/contracts/utils/math/SafeMath.sol";


contract MarketPlaceKSM is IERC721Receiver {
    using SafeMath for uint;
    struct Order {
        
        uint256 idNFT;
        uint256 currencyCode;
        uint256 price;
        uint256 time;
        address idCollection;
        address userAddr;
        uint8 flagActive;        
    }
    Order[] public  orders;

    mapping (address => mapping (uint256 => uint256)) public balanceKSM;  //  [userAddr] => [KSMs]
    mapping (address => mapping (uint256 => uint256)) public  asks ; // [buyer][idCollection][idNFT] => idorder

    mapping (address => uint[]) public asksbySeller; // [addressSeller] =>idorder

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
    * Make bids (orders) to sell NFTs 
    */
    function setAsk (uint256 _price, 
                    uint256  _currencyCode, 
                    address _idCollection, 
                    uint256 _idNFT,
                    uint8 _active ) public  { //
        
        require (IERC721(_idCollection).ownerOf(_idNFT) == msg.sender, "Not right token owner");
        uint orderID =  asks[_idCollection][_idNFT];
        if (orders.length == 0 || orders[orderID].idCollection == address(0)){
            orders.push(Order(        
                    _idNFT,
                    _currencyCode,
                    _price,
                    block.timestamp,
                    _idCollection,
                    msg.sender,
                    _active
                ));
            asks[_idCollection][_idNFT] = orders.length-1;
            asksbySeller[msg.sender].push(orders.length-1);
            } else //edit existing order
            {
                orders[asks[_idCollection][_idNFT]] = Order(        
                    orders[asks[_idCollection][_idNFT]].idNFT,
                    _currencyCode,
                    _price,
                    block.timestamp,
                    orders[asks[_idCollection][_idNFT]].idCollection,
                    msg.sender,
                    _active);
            }

            IERC721(_idCollection).transferFrom(msg.sender, address(this), _idNFT);
            
    }


    function deposit (uint256 _amount, uint256 _currencyCode, address _sender   ) public onlyEscrow {

        balanceKSM[_sender][_currencyCode]= balanceKSM[_sender][_currencyCode].add(_amount);

    }

    function buy (address _idCollection, uint256 _idNFT ) public {
        
        Order memory order = orders[ asks[_idCollection][_idNFT]];
        //1. reduce balance
        balanceKSM[msg.sender][order.currencyCode] = balanceKSM[msg.sender][order.currencyCode].sub( order.price, "Insuccificient KSMs funds");
        // 2. close order
        orders[ asks[_idCollection][_idNFT]].flagActive = 0;
        // 3. transfer NFT to buyer
        IERC721(_idCollection).transferFrom(address(this), msg.sender, _idNFT);


    }

    function withdraw (uint256 _amount, uint256 _currencyCode, address _sender   ) public  onlyEscrow returns (bool ){
        balanceKSM[_sender][_currencyCode] = balanceKSM[_sender][_currencyCode].sub( _amount, "Insuccificient KSMs balance");
        return true;


    }

    function onERC721Received(address operator, address from, uint256 tokenId, bytes calldata data)  public override pure returns(bytes4) {
            return bytes4(keccak256("onERC721Received(address,address,uint256,bytes)"));
        }
}