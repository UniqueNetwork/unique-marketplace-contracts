const config = {
  wsEndpoint : 'wss://kusama-rpc.polkadot.io',

  adminSeed : ADMIN_SEED || '//Alice',

  dbHost : DB_HOST || 'localhost',
  dbPort : DB_PORT || 5432,
  dbName : DB_NAME|| 'marketplace',
  dbUser : DB_USER || 'marketplace',
  dbPassword : DB_PASSWORD || '12345'
};

module.exports = config;