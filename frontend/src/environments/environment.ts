export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  apiGatewayUrl: 'http://localhost:5000',
  stripePublishableKey: 'pk_test_your_stripe_key_here',
  appName: 'SaaS DocPay System',
  version: '1.0.0',
  features: {
    enableAnalytics: false,
    enablePushNotifications: false,
    enableBetaFeatures: true
  },
  auth: {
    tokenKey: 'saas_docpay_token',
    refreshTokenKey: 'saas_docpay_refresh_token',
    tokenExpirationBuffer: 300000 // 5 minutes in milliseconds
  }
};
