# marketplace_backend

 - install truffle, ganache  globally:
npm i -g truffle ganache-cli

 For testing
  - run ganache: ganache-cli -m "clutch captain shoe salt awake harvest setup primary inmate ugly among become" -i 999 -p 8545 -u 0xa0df350d2637096571F7A701CBc1C5fdE30dF76A --db ../ganache_local  -g 1025e9  -e 1000
  
- run: truffle test

For polka node testing:
start polka frontier node at port 9933, network_id have to be '8888'

- run: truffle migrate --network upt

Production config needs TBD
