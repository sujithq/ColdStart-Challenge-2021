const { getUser } = require('../shared/user-utils');

const data = require('../shared/catalog-data');

module.exports = async function (context, req) {
  try {
    console.log(' SQU 001');
    // Get the user details from the request
    const user = getUser(req);
    console.log(' SQU 002');
    // Create order
    // Get the pre-order info from the request
    const ret = {
      User: user.userDetails,
      IcecreamId: req.body.IcecreamId,
      Status: 'New',
      DriverId: null,
      FullAddress: '1 Microsoft Way, Redmond, WA 98052, USA',
      LastPosition: null,
    };
    console.log(' SQU 003');
    const id = await data.postOrder(ret);
    console.log(' SQU 004');
    ret.Id = id;
    context.res.status(201).send(ret);

  } catch (error) {
    console.log(error);
    context.res.status(500).send(error);
  }
};