const delay = function (ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

const getTime = function () {
  const date = new Date();
  return date.toISOString()?.split('T')[1]?.split('.')[0];
}

const getDay = function() {
  const date = new Date();
  return date.toISOString()?.split('T')[0];
}


module.exports = {
  delay,
  getTime,
  getDay,
}