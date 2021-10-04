// SPDX-License-Identifier:  Apache License
pragma solidity >= 0.8.0;
import "@openzeppelin/contracts/token/ERC721/IERC721.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/token/ERC721/IERC721Receiver.sol";
import "@openzeppelin/contracts/utils/math/SafeMath.sol";


contract MarketPlaceUNQ is IERC721Receiver {
    using SafeMath for uint;
    struct Order {
        
        uint256 idNFT;
        address currencyCode; //address of currency  token, = address(0) for UNQ
        uint256 price;
        uint256 time;
        address idCollection;
        address userAddr;
        uint8 flagActive;        
    }
    Order[] public  orders;

    mapping (address => mapping (uint256 => uint256)) public balanceKSM;  //  [userAddr] => [KSMs]
    mapping (address => mapping (uint256 => uint256)) public  asks ; // [idCollection][idNFT] => idorder

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
    function addAsk (uint256 _price, 
                    address  _currencyCode, 
                    address _idCollection, 
                    uint256 _idNFT
                     ) public  { //
        
        require (IERC721(_idCollection).ownerOf(_idNFT) == msg.sender, "Only token owner can make ask");
        
            orders.push(Order(        
                    _idNFT,
                    _currencyCode,
                    _price,
                    block.timestamp,
                    _idCollection,
                    msg.sender,
                    1 // 1 = is active
                ));
            asks[_idCollection][_idNFT] = orders.length-1;
            asksbySeller[msg.sender].push(orders.length-1);
            IERC721(_idCollection).transferFrom(msg.sender, address(this), _idNFT);
            
    }

    function editAsk (uint256 _price, 
                    address  _currencyCode, 
                    address _idCollection, 
                    uint256 _idNFT,
                    uint8  _active) public {
        require (IERC721(_idCollection).ownerOf(_idNFT) == msg.sender, "Only token owner can edit ask");

        uint orderID =  asks[_idCollection][_idNFT];
        require (orders[orderID].flagActive != 0, "This ask is closed");
        if (_price> 0 ) {
            orders[orderID].price = _price ;  
        }
        
        if (_currencyCode != address(0) ) {
            orders[orderID].currencyCode = _currencyCode ;  
        }

        orders[orderID].time = block.timestamp;
        orders[orderID].flagActive = _active;
        
        }



    function buy (address _idCollection, uint256 _idNFT ) public payable { //buing for UNQ like as ethers 
        
        Order memory order = orders[asks[_idCollection][_idNFT]]; 
        //1. check sent amount and send to seller
        require (msg.value == order.price, "Not right amount UNQ sent, have to be equal price" );     
        payable(order.userAddr).transfer(order.price); 
        // 2. close order
        orders[ asks[_idCollection][_idNFT]].flagActive = 0;
        // 3. transfer NFT to buyer
        IERC721(_idCollection).transferFrom(address(this), msg.sender, _idNFT);


    }


    function buy (address _idCollection, uint256 _idNFT, address _currencyCode, uint _amount ) public payable {
        
        Order memory order = orders[ asks[_idCollection][_idNFT]];
        //1. check sent amount and transfer from buyer to seller
        require (order.price == _amount && order.currencyCode == _currencyCode, "Not right amount or currency sent, have to be equal currency and price" );
        // !!! transfer have to be approved to marketplace!
        IERC20(order.currencyCode).transferFrom(msg.sender, address(this), order.price); //to not disclojure buyer's address 
        IERC20(order.currencyCode).transfer(order.userAddr, order.price);
        // 2. close order
        orders[ asks[_idCollection][_idNFT]].flagActive = 0;
        // 3. transfer NFT to buyer
        IERC721(_idCollection).transferFrom(address(this), msg.sender, _idNFT);


    }


    function onERC721Received(address operator, address from, uint256 tokenId, bytes calldata data)  public override pure returns(bytes4) {
            return bytes4(keccak256("onERC721Received(address,address,uint256,bytes)"));
        }
}