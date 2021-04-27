const data = require('../shared/order-cosmos-data');
const driverData = require('../shared/driver-data');

module.exports = async function (context, req) {
  try {
    //Log Body
    // console.log(req.body);
    // context.res.status(200).send(req.body);

    // Retrieve the order id from the url
    const orderId = req.params.Id;

    // let msg = 'Order Id:' + orderId;
    // console.log(msg);
    // context.res.status(200).send(msg);

    const status = req.body.status;
    // let msg = 'Order Status:' + status;
    // console.log(msg);
    // context.res.status(200).send(msg);

    const driverId = req.body.driver.driverId;
    // let msg = '\nOrder Driver Id:' + driverId;
    // console.log(msg);
    // context.res.status(200).send(msg);

    const lastPosition = req.body.lastPosition;
    // let msg = '\nLast Position:' + lastPosition;
    // console.log(msg);
    // context.res.status(200).send(msg);

    let driverName = null;
    let driverImageUrl = null;

    // Get the order
    // console.log('Retrieving order: ' + orderId);
    const order = await data.getOrderById(orderId);
    // console.log(order);
    // context.res.status(200).send(req.body + '\n' + order);

    if (driverId) {
      // console.log('Driver: ' + driverId);

      // Get driver details
      const driver = await driverData.getDriverById(driverId);
      // console.log(driver);

      // Update order driver details
      driverName = driver.length > 0 ? driver[0].name : null;
      driverImageUrl = driver.length > 0 ? driver[0].imageUrl : null;

      // console.log(`Driver ${driverName} ${driverImageUrl}`);
    }

    // console.log('Updating order');

    let updated = await data.updateOrder(order, status, lastPosition, driverId, driverName, driverImageUrl);

    // console.log('Done updating order');

    context.res.status(200).send(updated);
  } catch (error) {
    context.res.status(500).send(error);
  }
};
