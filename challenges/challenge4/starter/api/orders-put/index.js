const data = require('../shared/order-cosmos-data');
const driverData = require('../shared/driver-data');

module.exports = async function (context, req) {
  try {
    const orderId = req.params.Id;
    const status = req.body.status;
    const driverId = req.body.driver.driverId;
    const lastPosition = req.body.lastPosition;

    let driverName = null;
    let driverImageUrl = null;

    const order = await data.getOrderById(orderId);

    if (driverId) {
      const driver = await driverData.getDriverById(driverId);

      driverName = driver.length > 0 ? driver[0].name : null;
      driverImageUrl = driver.length > 0 ? driver[0].imageUrl : null;

    }

    let updated = await data.updateOrder(order, status, lastPosition, driverId, driverName, driverImageUrl);

    context.res.status(200).send(updated);
  } catch (error) {
    context.res.status(500).send(error);
  }
};
