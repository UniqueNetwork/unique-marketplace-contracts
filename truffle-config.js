require('dotenv').config();

const pkOnwer = process.env.APP_OWNER_PRIVATE_KEY;
const pkEscrow = process.env.APP_ESCROW_PRIVATE_KEY;
const pktestbuyer = process.env.TEST_BUYER_PRIVATE_KEY;
const pktestseller = process.env.TEST_SELLER_PRIVATE_KEY;
const endpoint = "https://rpc-opal.unique.network/" || process.env.ENDPOINT;

/**
 * Use this file to configure your truffle project. It's seeded with some
 * common settings for different networks and features like migrations,
 * compilation and testing. Uncomment the ones you need or modify
 * them to suit your project as necessary.
 *
 * More information about configuration can be found at:
 *
 * trufflesuite.com/docs/advanced/configuration
 *
 * To deploy via Infura you'll need a wallet provider (like @truffle/hdwallet-provider)
 * to sign your transactions before they're sent to a remote public node. Infura accounts
 * are available for free at: infura.io/register.
 *
 * You'll also need a mnemonic - the twelve word phrase the wallet uses to generate
 * public/private key pairs. If you're publishing your code to GitHub make sure you load this
 * phrase from a file you've .gitignored so it doesn't accidentally become public.
 *
 */

// const HDWalletProvider = require('@truffle/hdwallet-provider');
// const infuraKey = "fj4jll3k.....";
//
// const fs = require('fs');
// const mnemonic = fs.readFileSync(".secret").toString().trim();
const HDWalletProvider = require("@truffle/hdwallet-provider");

module.exports = {
  /**
   * Networks define how you connect to your ethereum client and let you set the
   * defaults web3 uses to send transactions. If you don't specify one truffle
   * will spin up a development blockchain for you on port 9545 when you
   * run `develop` or `test`. You can ask a truffle command to use a specific
   * network from the command line, e.g
   *
   * $ truffle test --network <network-name>
   */

  networks: {
    development: {
    
      host: "127.0.0.1",     // Localhost (default: none)
      port: 8545,            // Standard Ethereum port (default: none)
      network_id: "*",            
        /**
* ganache-cli -m "clutch captain shoe salt awake harvest setup primary inmate ugly among become" -i 999 -p 8545 -u 0xa0df350d2637096571F7A701CBc1C5fdE30dF76A --db ../ganache_local3 --allowUnlimitedContractSize  -g 0 -e 1000
*/
      },
   
    upt: { // Unique Public testnet

        provider: () => new HDWalletProvider({
            privateKeys: [pkOnwer,pkEscrow, pktestbuyer, pktestseller],  
            providerOrUrl: endpoint}),/* http://35.157.131.180:9973/  http://15.236.177.137:9833/*/
            network_id: 8888,
           // gasPrice: '0xEEA39D5A99'
            /** Eth: 0xf1a477099Ef8aA0f096be09A4CBBA858da993c41  
             * pk eth 3f13f692c4b88b5a6a317e6583623443658e37f9b7cbcbc54267b7b4d1c3f54c
             *             * 
             * eth 0xa0df350d2637096571F7A701CBc1C5fdE30dF76A  
             * pk 0xb8c1b5c1d81f9475fdf2e334517d29f733bdfa40682207571b12fc1142cbf329
             * polka 5H9kK46KadoBKXGbhufTcB6ujAMfuQYt7zGB9rjEV6KumKzi
    * ganache-cli -m "clutch captain shoe salt awake harvest setup primary inmate ugly among become" -i 999 -p 8545 -u 0xa0df350d2637096571F7A701CBc1C5fdE30dF76A --db ../ganache_local3 --allowUnlimitedContractSize  -g 0 -e 1000
    */
          },
    uptloc: { // Unique Public testnet

            provider: () => new HDWalletProvider({
                privateKeys: [pkOnwer,pkEscrow, pktestbuyer, pktestseller],  
                providerOrUrl: 'http://127.0.0.1:8545'}),/* http://35.157.131.180:9973/  http://15.236.177.137:9833/*/
                network_id: 8888,
               // gasPrice: '0xEEA39D5A99'
                /** Eth: 0xf1a477099Ef8aA0f096be09A4CBBA858da993c41  
                 * pk eth 3f13f692c4b88b5a6a317e6583623443658e37f9b7cbcbc54267b7b4d1c3f54c
                 *             * 
                 * eth 0xa0df350d2637096571F7A701CBc1C5fdE30dF76A  
                 * pk 0xb8c1b5c1d81f9475fdf2e334517d29f733bdfa40682207571b12fc1142cbf329
                 * polka 5H9kK46KadoBKXGbhufTcB6ujAMfuQYt7zGB9rjEV6KumKzi
        * ganache-cli -m "clutch captain shoe salt awake harvest setup primary inmate ugly among become" -i 888 -p 8545 -u 0xa0df350d2637096571F7A701CBc1C5fdE30dF76A --db ../ganache_UPT -f https://rpc-opal.unique.network/   -e 1000
        */
    },
    // Useful for testing. The `development` name is special - truffle uses it by default
    // if it's defined here and no other network is specified at the command line.
    // You should run a client (like ganache-cli, geth or parity) in a separate terminal
    // tab if you use this network and you must also set the `host`, `port` and `network_id`
    // options below to some value.
    //
    // development: {
    //  host: "127.0.0.1",     // Localhost (default: none)
    //  port: 8545,            // Standard Ethereum port (default: none)
    //  network_id: "*",       // Any network (default: none)
    // },
    // Another network with more advanced options...
    // advanced: {
    // port: 8777,             // Custom port
    // network_id: 1342,       // Custom network
    // gas: 8500000,           // Gas sent with each transaction (default: ~6700000)
    // gasPrice: 20000000000,  // 20 gwei (in wei) (default: 100 gwei)
    // from: <address>,        // Account to send txs from (default: accounts[0])
    // websocket: true        // Enable EventEmitter interface for web3 (default: false)
    // },
    // Useful for deploying to a public network.
    // NB: It's important to wrap the provider as a function.
    // ropsten: {
    // provider: () => new HDWalletProvider(mnemonic, `https://ropsten.infura.io/v3/YOUR-PROJECT-ID`),
    // network_id: 3,       // Ropsten's id
    // gas: 5500000,        // Ropsten has a lower block limit than mainnet
    // confirmations: 2,    // # of confs to wait between deployments. (default: 0)
    // timeoutBlocks: 200,  // # of blocks before a deployment times out  (minimum/default: 50)
    // skipDryRun: true     // Skip dry run before migrations? (default: false for public nets )
    // },
    // Useful for private networks
    // private: {
    // provider: () => new HDWalletProvider(mnemonic, `https://network.io`),
    // network_id: 2111,   // This network is yours, in the cloud.
    // production: true    // Treats this network as if it was a public net. (default: false)
    // }
  },

  // Set default mocha options here, use special reporters etc.
  mocha: {
    // timeout: 100000
  },

  // Configure your compilers
  compilers: {
    solc: {
       version: "0.8.4",    // Fetch exact version from solc-bin (default: truffle's version)
      // docker: true,        // Use "0.5.1" you've installed locally with docker (default: false)
      // settings: {          // See the solidity docs for advice about optimization and evmVersion
      //  optimizer: {
      //    enabled: false,
      //    runs: 200
      //  },
      //  evmVersion: "byzantium"
      // }
    }
  }
};
