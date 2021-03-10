const { getUser } = require('../shared/user-utils');

var Connection = require('tedious').Connection;
var Request = require('tedious').Request;
var TYPES = require('tedious').TYPES;

const { config } = require('../shared/config');


const server = process.env.db_server;
const userName = process.env.db_user;
const password = process.env.db_password;
const database = process.env.db_database;

const executeSQL = (context, req) => {
  // console.log('Step 1');
  // Get the user details from the request
  const user = getUser(req);

  console.log(req.body);

  // Create order
  // Get the pre-order info from the request
  const ret = {
    User: user.userDetails,
    // Date: new Date().toISOString(),
    IcecreamId: req.body.IcecreamId,
    Status: 'New',
    DriverId: null,
    FullAddress: '1 Microsoft Way, Redmond, WA 98052, USA',
    LastPosition: null,
  };

  // Create Connection object
  const connection = new Connection({
    server: config.db_server,
    authentication: {
      type: 'default',
      options: {
        userName: config.db_user,
        password: config.db_password,
      }
    },
    options: {
      database: config.db_database,
      encrypt: true
    }
  });

  // Create the command to be executed
  const request = new Request(`INSERT Orders ([User], [Date], [IcecreamId], [Status], [DriverId], [FullAddress], [LastPosition]) OUTPUT INSERTED.Id VALUES (@User, CURRENT_TIMESTAMP, @IcecreamId, @Status, @DriverId, @FullAddress, @LastPosition)`, (err, res) => {
    if (err) {
      context.log.error(err);
      context.res.status = 500;
      context.res.body = "Error executing T-SQL command";
      return;
    }
    else {
      ret.Id = id;
      context.res.status = 201;
      context.res.body = JSON.stringify(ret);
    }
    context.done();
  });

  // Add Param Values
  request.addParameter('User', TYPES.NVarChar, ret.User);
  request.addParameter('Status', TYPES.NVarChar, ret.Status);
  request.addParameter('IcecreamId', TYPES.Int, ret.IcecreamId);
  request.addParameter('FullAddress', TYPES.NVarChar, ret.FullAddress);
  request.addParameter('LastPosition', TYPES.NVarChar, null);
  request.addParameter('DriverId', TYPES.Int, null);


  // Handle 'connect' event
  connection.on('connect', err => {
    if (err) {
      context.log.error(err);
      context.res.status = 500;
      context.res.body = "Error connecting to Azure SQL query";
      context.done();
    }
    else {
      connection.execSql(request);
    }
  });

  // Result Id
  var id = null;

  // Handle 'row' event
  request.on('row', function (columns) {
    columns.forEach(function (column) {
      if (column.value === null) {
      } else {
        id = column.value;
      }
    });
  });

  request.on('done', function (rowCount, more, rows) {
    return id;
  })

  // Connect
  connection.connect();
}

module.exports = function (context, req) {
  executeSQL(context, req);
}