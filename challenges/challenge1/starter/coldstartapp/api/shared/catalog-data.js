const { executeSQL } = require('./sql-utils');

getCatalog = async function () {
  return await executeSQL(`SELECT Id, Name, Description, ImageUrl FROM Icecreams`);
}

module.exports = { getCatalog };