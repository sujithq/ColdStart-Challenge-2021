const { getUser } = require('../shared/user-utils');
const { config } = require('../shared/config');

// Db Stuff
var Connection = require('tedious').Connection;

// Env Vars
const server = config.db_server;
const userName = config.db_user;
const password = config.db_password;
const database = config.db_database;

module.exports = async function (context, req) {
  // Get the user details from the request
  const user = getUser(req);
  console.log(user);
  console.log(req.body);
  // Create order
  // Get the pre-order from the request
  const ret = {
    User: user.userDetails,
    // Date: new Date().toISOString(),
    IcecreamId: req.body.IcecreamId,
    Status: 'New',
    DriverId: null,
    FullAddress: '1 Microsoft Way, Redmond, WA 98052, USA',
    LastPosition: null,
  };
  console.log(ret);
  var cfg = {  
    server: config.db_server,
    authentication: {
        type: 'default',
        options: {
            userName: config.db_user,
            password: config.db_password
        }
    },
    options: {
        // If you are on Microsoft Azure, you need encryption:
        encrypt: true,
        database: config.db_database
    }
  };  
  console.log(cfg);

  var connection = new Connection(cfg);

  // console.log(connection);

  // var retId = null;

  connection.on('connect', function(err) {  
    // If no error, then good to proceed. 
    if(!err){ 
      console.log("connect ok");  
      executeStatement1(context, ret);
      console.log('After executeStatement1');
    }else{
      console.log("connect error");
      context.res = {
        status: 500,
        body: err
      };  
      context.done();
    }
  });
  connection.connect();

  var Request = require('tedious').Request;
  var TYPES = require('tedious').TYPES;

  function executeStatement1(context, ret) {  
    console.log('Begin executeStatement1');
    console.log(ret);

    var request = new Request("INSERT Orders ([User], [Date], [IcecreamId], [Status], [DriverId], [FullAddress], [LastPosition]) OUTPUT INSERTED.Id VALUES (@User, CURRENT_TIMESTAMP, @IcecreamId, @Status, @DriverId, @FullAddress, @LastPosition);", function(err) {  
      if (err) {  
        console.log('Error!');
        console.log(err);
      }
      else{
        console.log('Ok');
        console.log(res);
      }
    });

    request.addParameter('User', TYPES.NVarChar, ret.User);  
    request.addParameter('Status', TYPES.NVarChar, ret.Status);  
    request.addParameter('IcecreamId', TYPES.Int, ret.IcecreamId);  
    request.addParameter('FullAddress', TYPES.NVarChar, ret.FullAddress);  
    request.addParameter('LastPosition', TYPES.NVarChar, null);  
    request.addParameter('DriverId', TYPES.Int, null);  

    request.on('row', function(columns) {  
      columns.forEach(function(column) {  
        if (column.value === null) {  
          console.log('NULL');  
        } else {  
          console.log("Product id of inserted item is " + column.value);
          context.res.body.Id = column.value;
        }  
      });  
    });       

    connection.execSql(request);
  }
  context.res.status = 201;
  context.res.body = ret;
  context.done();
};