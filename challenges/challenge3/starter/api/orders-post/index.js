const { getUser } = require('../shared/user-utils');

const data = require('../shared/catalog-data');

var uuid = require('uuid');

module.exports = async function (context, req) {
  try {
    // Get the user details from the request
    const user = getUser(req);
    // Create order
    // Get the pre-order info from the request
    const ret = {
      Id: uuid.v4(),
      Date: new Date().toISOString(),
      User: user.userDetails,
      IcecreamId: req.body.IcecreamId,
      Status: 'New',
      DriverId: null,
      FullAddress: req.body.ShippingAddress,
      LastPosition: null,
    };
    console.log(ret);
    const id = await data.postOrder(ret);
    ret.Id = id;
    console.log('Queueing order');
    console.log(ret);
    context.bindings.myQueueItem = JSON.stringify(ret);

    context.res.status(201).send(ret);

  } catch (error) {
    context.error(error);
    context.res.status(500).send(error);
  }
};