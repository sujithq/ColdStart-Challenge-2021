var Connection = require('tedious').Connection;
var Request = require('tedious').Request;
var TYPES = require('tedious').TYPES;

const { config } = require('../shared/config');


const server = process.env.db_server;
const userName = process.env.db_user;
const password = process.env.db_password;
const database = process.env.db_database;

const executeSQL = (context) => {
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
    const request = new Request(`SELECT Id, Name, Description, ImageUrl FROM Icecreams`, (err, results) => {
        if (err) {
            context.log.error(err);            
            context.res.status = 500;
            context.res.body = "Error executing T-SQL command";
            return;
        }
        else {
          context.log(results);
          context.res = {
              body: JSON.stringify(result)
          }   
        }
        context.done();
    });    

    // Handle 'connect' event
    connection.on('connect', err => {
        if (err) {
            context.log.error(err);              
            context.res.status = 500;
            context.res.body = "Error connecting to Azure SQL query";
            context.done();
        }
        else {
            // Connection succeeded so execute T-SQL stored procedure
            // if you want to executer ad-hoc T-SQL code, use connection.execSql instead
            connection.execSql(request);
        }
    });

    // Result object
    var result = [];

    // Handle 'row' event
    request.on('row', function(columns) {
      var obj = {}  
      columns.forEach( function(column) {
        if(column.value !== null){
         var key = column.metadata.colName
         var val = column.value
          obj[key] = val
        }
      });
      result.push(obj)
    });
 
    request.on('done', function ( rowCount, more, rows) {
      return result;
    })

    // Connect
    connection.connect();
}

module.exports = function (context) {    
    executeSQL(context);
}