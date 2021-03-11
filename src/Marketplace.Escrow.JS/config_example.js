const config = {
  wsEndpoint : 'wss://kusama-rpc.polkadot.io',

  adminSeed : '//Alice',
  adminAddress : 'HNfA5wkUPyVvz1miAeyNcp59cBEs4zLCDbcRAWptmjR89Q4',

  dbHost : process.env.DB_HOST || 'localhost',
  dbPort : process.env.DB_PORT || 5432,
  dbName : process.env.DB_NAME || 'marketplace',
  dbUser : process.env.DB_USER || 'marketplace',
  dbPassword : process.env.DB_PASSWORD || '12345'
};

module.exports = config;