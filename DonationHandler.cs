using Android.App;
using Android.BillingClient.Api;
using Android.Content;
using Android.Util;
using Java.Interop;
using SoftWing.System;
using System;
using System.Collections.Generic;

namespace SoftWing
{
    class DonationHandler : Java.Lang.Object, IPurchasesUpdatedListener, System.MessageSubscriber, IBillingClientStateListener, ISkuDetailsResponseListener
    {
        private const String TAG = "DonationHandler";
        private MessageDispatcher dispatcher;
        private BillingClient billingClient;
        private IList<SkuDetails> skuDetails = null;

        public DonationHandler()
        {
            dispatcher = MessageDispatcher.GetInstance();
            billingClient = BillingClient.NewBuilder(Application.Context)
                .SetListener(this)
                .EnablePendingPurchases()
                .Build();

        }

        public void Start()
        {
            billingClient.StartConnection(this);

            var skuList = new List<String>();
            skuList.Add("Feed the Engineer");
            SkuDetailsParams.Builder details_builder = SkuDetailsParams.NewBuilder();
            details_builder.SetSkusList(skuList).SetType(BillingClient.SkuType.Inapp);
            billingClient.QuerySkuDetails(details_builder.Build(), this);
        }

        public void OnBillingServiceDisconnected()
        {
            Log.Debug(TAG, "OnBillingServiceDisconnected()");
        }

        public void OnBillingSetupFinished(BillingResult result)
        {
            Log.Debug(TAG, "OnBillingSetupFinished()");
            if (result.ResponseCode != BillingResponseCode.Ok)
            {
                Log.Debug(TAG, "Billing setup failed with code " + result.ResponseCode.ToString());
            }
        }

        public void LaunchBilling(SkuDetails sku, Activity parent)
        {
            Log.Debug(TAG, "LaunchBilling()");
            BillingFlowParams billingFlowParams = BillingFlowParams.NewBuilder()
                .SetSkuDetails(sku)
                .Build();
            var responseCode = billingClient.LaunchBillingFlow(parent, billingFlowParams).ResponseCode;
            if (responseCode == BillingResponseCode.Ok)
            {
                // Thank the foolish mortal
            }
        }

        public void OnPurchasesUpdated(BillingResult result, IList<Purchase> purchases)
        {
            Log.Debug(TAG, "OnPurchasesUpdated()");
            if (result.ResponseCode == BillingResponseCode.Ok && purchases != null)
            {
                foreach (Purchase purchase in purchases)
                {
                    //handlePurchase(purchase);
                }
            }
            else if (result.ResponseCode == BillingResponseCode.UserCancelled)
            {
                // Handle an error caused by a user cancelling the purchase flow.
            }
            else
            {
                // Handle any other error codes.
            }

        }

        public void Accept(SystemMessage message)
        {
            Log.Debug(TAG, "Accept()");
        }

        public void OnSkuDetailsResponse(BillingResult result, IList<SkuDetails> sku_list)
        {
            Log.Debug(TAG, "OnSkuDetailsResponse()");
            if (result.ResponseCode != BillingResponseCode.Ok)
            {
                Log.Debug(TAG, "SKU Detail setup failed with code " + result.ResponseCode.ToString());
                return;
            }
            skuDetails = sku_list;
            Log.Debug(TAG, skuDetails.ToString());
        }
    }
}