export const environment = {
  production: false,
  msalConfig: {
    auth: {
      clientId: '87b8b43b-ad6b-40da-9461-14c7357ecd54',
      authority: 'https://oportunizaapp.ciamlogin.com/3af5d8b9-4cdf-4cb3-819e-ef412714d63f',
    },
  },
  apiConfig: {
    scopes: ['api://a863e08f-99f4-4e08-ae28-afbc4d562269/oportuniza.read'],
    uri: 'http://localhost:5000/',
  },
  keycloak: {
    url: 'http://localhost:9090',
    realm: 'oportuniza',
    clientId: 'oportuniza-client',
    secret:"Sr1LFcfOHwtFckn8HAHHAf7IxklDiBI3"
    // secret:"DS2qkXNCVoyuTgeL9Ke2OA0Srvha6JNB"
  },
  apiUrl: "http://localhost:5000/api",
  apiUrlNormal: "http://localhost:5000"
};
