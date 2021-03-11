const config = {
  wsEndpoint : 'wss://kusama-rpc.polkadot.io',

  adminSeed : process.env.ADMIN_SEED || '//Alice',

  dbHost : process.env.DB_HOST || 'localhost',
  dbPort : process.env.DB_PORT || 5432,
  dbName : process.env.DB_NAME|| 'marketplace',
  dbUser : process.env.DB_USER || 'marketplace',
  dbPassword : process.env.DB_PASSWORD || '12345'
};

module.exports = config;