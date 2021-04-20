const storageBaseUrl = "https://coldstartsa.blob.core.windows.net/web/assets/";

// Schema for drivers
const data = {
  drivers: [
    {
      driverId: 1,
      name: 'Daisy Driver',
      imageUrl: storageBaseUrl + 'Driver1.png'
    },
    {
      driverId: 2,
      name: 'Donny Driver',
      imageUrl: storageBaseUrl + 'Driver1.png'
    },
  ],
};

/**
 * Get a driver by it's id
 * @param {integer} driverId 
 * @returns 
 */
function getDriverById(driverId) {
  return data.drivers.filter(function(driver) {
    return driver.driverId == driverId;
  });
}

/**
 * Get a driver by it's id
 * @param {integer} driverId 
 * @returns 
 */
function getDrivers() {
  return data.drivers;
}

module.exports = { getDriverById, getDrivers };
