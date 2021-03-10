const { getAuthenticationStatus } = require('../shared/user-utils');

// const { getCatalog } = require('../shared/catalog-data');

const { config } = require('../shared/config');

const { v1: uuidv1 } = require('uuid');
const Personalizer = require('@azure/cognitiveservices-personalizer');
const CognitiveServicesCredentials = require('@azure/ms-rest-azure-js').CognitiveServicesCredentials;

const serviceKey = config.personalizer_key;

// The endpoint specific to your personalization service instance; 
// e.g. https://<your-resource-name>.cognitiveservices.azure.com
const baseUri = config.personalizer_base;

const credentials = new CognitiveServicesCredentials(serviceKey);

// Initialize Personalization client.
const personalizerClient = new Personalizer.PersonalizerClient(credentials, baseUri);

function getContextFeaturesList(req) {

  const dayOfWeekFeatures = ['su', 'mo', 'tu', 'we', 'th', 'fr', 'sa'];
  const timeOfDayFeatures = ['morning', 'afternoon', 'evening', 'night'];

  let authenticationStatus = getAuthenticationStatus(req);

  const browser = require('browser-detect');

  let date = new Date();
  let dow = date.getDay();
  let tod = date.getHours();
  let t = timeOfDayFeatures[0];

  if (tod > 6 && tod <= 12) {
    t = timeOfDayFeatures[0];
  } else if (tod > 12 && tod <= 18) {
    t = timeOfDayFeatures[1];
  } else if (tod > 18 && tod <= 22) {
    t = timeOfDayFeatures[2];
  } else {
    t = timeOfDayFeatures[3];
  }

  const b = browser(req.headers['user-agent']);

  return [
    {
      "time": t
    },
    {
      "day": dayOfWeekFeatures[dow]
    },
    {
      "browser": b.name
    },
    {
      "status": authenticationStatus
    }
  ];
}

function getActionsList(context) {
  // const c = getCatalog(context);
  // console.log(c);

  return [
    {
      "id": "1",
      "features": [
        {
          "Name": "Color Pop",
          "Description": "Delicious 4-color popsicle, plenty of vitamins.",
          "ImageUrl": "https://coldstartsa.blob.core.windows.net/web/assets/Icecream1.png"
        }
      ]
    },
    {
      "id": "2",
      "features": [
        {
          "Name": "Lemoncella",
          "Description": "Refreshing lemon-flavoured icecream bar.",
          "ImageUrl": "https://coldstartsa.blob.core.windows.net/web/assets/Icecream2.png"
        }
      ]
    },
    {
      "id": "3",
      "features": [
        {
          "Name": "Pink Panther",
          "Description": "Fruity ice cream bar with hints of strawberry and lime.",
          "ImageUrl": "https://coldstartsa.blob.core.windows.net/web/assets/Icecream3.png"
        }
      ]
    },
    {
      "id": "4",
      "features": [
        {
          "Name": "Choco Chique",
          "Description": "Filled with praline and covered with the finest Belgian chocolate.",
          "ImageUrl": "https://coldstartsa.blob.core.windows.net/web/assets/Icecream4.png"
        }
      ]
    },
    {
      "id": "5",
      "features": [
        {
          "Name": "Blue Lagoon",
          "Description": "Blueberry and melon ice cream bar.",
          "ImageUrl": "https://coldstartsa.blob.core.windows.net/web/assets/Icecream5.png"
        }
      ]
    },
    {
      "id": "6",
      "features": [
        {
          "Name": "Purple Rain",
          "Description": "Indulging strawberry and vodka icecream bar.",
          "ImageUrl": "https://coldstartsa.blob.core.windows.net/web/assets/Icecream6.png"
        }
      ]
    },
    {
      "id": "7",
      "features": [
        {
          "Name": "Sorbonne",
          "Description": "Strawberry and raspberry sorbet.",
          "ImageUrl": "https://coldstartsa.blob.core.windows.net/web/assets/Icecream7.png"
        }
      ]
    },
    {
      "id": "8",
      "features": [
        {
          "Name": "Sandstorm",
          "Description": "Chocolate and vanille ice cream cookie (3).",
          "ImageUrl": "https://coldstartsa.blob.core.windows.net/web/assets/Icecream8.png"
        }
      ]
    },
    {
      "id": "9",
      "features": [
        {
          "Name": "Maxi jazz",
          "Description": "Dame Blanche flavoured ice cream cake (6p).",
          "ImageUrl": "https://coldstartsa.blob.core.windows.net/web/assets/Icecream9.png"
        }
      ]
    },
    {
      "id": "10",
      "features": [
        {
          "Name": "Triplets",
          "Description": "Surprise yourself with a random selection of 3 different flavors.",
          "ImageUrl": "https://coldstartsa.blob.core.windows.net/web/assets/Icecream10.png"
        }
      ]
    }
  ];
}


module.exports = async function (context, req) {

  let rankRequest = {}

  // Generate an ID to associate with the request.
  rankRequest.eventId = uuidv1();

  // Get context information from the user.
  rankRequest.contextFeatures = getContextFeaturesList(req);

  // Get the actions list to choose from personalization with their features.
  rankRequest.actions = getActionsList(context);

  // Exclude an action for personalization ranking. This action will be held at its current position.
  //rankRequest.excludedActions = getExcludedActionsList();

  rankRequest.deferActivation = false;

  // Rank the actions
  const rankResponse = await personalizerClient.rank(rankRequest);

  console.log(rankResponse);

  context.res.body = JSON.stringify(rankResponse);
  context.done();
}