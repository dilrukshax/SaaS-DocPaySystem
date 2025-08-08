export const environment = {
  production: true,
  apiUrl: 'https://api.docpaysystem.com/api',
  apiGatewayUrl: 'https://api.docpaysystem.com',
  stripePublishableKey: 'pk_live_your_stripe_key_here',
  appName: 'SaaS DocPay System',
  version: '1.0.0',
  features: {
    enableAnalytics: true,
    enablePushNotifications: true,
    enableBetaFeatures: false
  },
  auth: {
    tokenKey: 'saas_docpay_token',
    refreshTokenKey: 'saas_docpay_refresh_token',
    tokenExpirationBuffer: 300000 // 5 minutes in milliseconds
  }
};
