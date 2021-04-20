const data = require('../shared/catalog-data');

module.exports = async function (context) {
  try {
    var items = [];

    if(context.bindingData.id){
      items = await data.getCatalogById(context.bindingData.id);
    }else{
      items = await data.getCatalog();
    }
    context.res.status(200).send(items);
  } catch (error) {
    context.res.status(500).send(error);
  }
};