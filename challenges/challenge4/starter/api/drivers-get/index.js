const data = require('../shared/drivers-data');

module.exports = async function (context) {
  try {
    var items = [];

    if(context.bindingData.id){
      items = await data.getDriverById(context.bindingData.id);
    }else{
      items = await data.getDrivers();
    }
    context.res.status(200).send(items);
  } catch (error) {
    context.res.status(500).send(error);
  }
};