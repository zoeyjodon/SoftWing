using Android.App;
using Android.BillingClient.Api;
using Android.Util;
using Android.Views;
using Android.Widget;
using SoftWing.System;
using System;
using System.Collections.Generic;

namespace SoftWing
{
    class DonationHandler : Java.Lang.Object, IPurchasesUpdatedListener, MessageSubscriber, IBillingClientStateListener, ISkuDetailsResponseListener, Spinner.IOnItemSelectedListener
    {
        private const String TAG = "DonationHandler";
        private MessageDispatcher dispatcher;
        private BillingClient billingClient;
        private List<String> skuList = new List<String>() { "donate_1", "donate_5", "donate_10", "donate_20", "donate_50", "donate_100" };
        public List<SkuDetails> skuDetails = new List<SkuDetails>();
        private int ignore_keyset_count = 0;
        private Activity parent_activity;

        public DonationHandler(Activity parent)
        {
            parent_activity = parent;
            dispatcher = MessageDispatcher.GetInstance();
            billingClient = BillingClient.NewBuilder(Application.Context)
                .SetListener(this)
                .EnablePendingPurchases()
                .Build();
        }

        public void Start()
        {
            billingClient.StartConnection(this);
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
            SkuDetailsParams.Builder details_builder = SkuDetailsParams.NewBuilder();
            details_builder.SetSkusList(skuList).SetType(BillingClient.SkuType.Inapp);
            billingClient.QuerySkuDetails(details_builder.Build(), this);
        }

        private void LaunchBilling(SkuDetails sku, Activity parent)
        {
            Log.Debug(TAG, "LaunchBilling()");
            BillingFlowParams billingFlowParams = BillingFlowParams.NewBuilder()
                .SetSkuDetails(sku)
                .Build();
            billingClient.LaunchBillingFlow(parent, billingFlowParams);
        }

        public void OnPurchasesUpdated(BillingResult result, IList<Purchase> purchases)
        {
            Log.Debug(TAG, "OnPurchasesUpdated()");
            if (result.ResponseCode == BillingResponseCode.Ok && purchases != null)
            {
                string message = "Thank you! Your donation will have the following effect:\n";
                string sku = purchases[0].Skus[0];
                foreach (var item in skuDetails)
                {
                    if (item.Sku == sku)
                    {
                        message += item.Description;
                        break;
                    }
                }
                var toast = Toast.MakeText(parent_activity, message, ToastLength.Long);
                toast.Show();
            }
            else if (result.ResponseCode != BillingResponseCode.UserCancelled)
            {
                var toast = Toast.MakeText(parent_activity, "Something went wrong, try again maybe?", ToastLength.Long);
                toast.Show();
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
            skuDetails.AddRange(sku_list);
            skuDetails.Sort((p, q) => p.PriceAmountMicros.CompareTo(q.PriceAmountMicros));
            dispatcher.Post(new System.Messages.DonationUpdateMessage(System.Messages.DonationUpdateMessage.UpdateType.SetupComplete));
        }

        public Spinner CreateDonationSpinner()
        {
            Log.Debug(TAG, "CreateDonationSpinner");
            var spinner = new Spinner(parent_activity);
            spinner.Prompt = "Select a Donation Tier";
            spinner.Clickable = true;
            GridLayout.LayoutParams layout_params = new GridLayout.LayoutParams();
            layout_params.Width = 0;
            layout_params.Height = 0;
            layout_params.ColumnSpec = GridLayout.InvokeSpec(0, 3);
            layout_params.RowSpec = GridLayout.InvokeSpec(0, 1);
            spinner.LayoutParameters = layout_params;

            List<string> itemTitles = new List<string>();
            itemTitles.Add("Give The Engineer Nothing ($0)");
            foreach (var item in skuDetails)
            {
                if (!itemTitles.Contains(item.Title))
                {
                    // Remove the app specific stuff
                    itemTitles.Add(item.Title.Split(" (com.")[0]);
                }
            }
            var adapter = new ArrayAdapter<string>(parent_activity, Android.Resource.Layout.SimpleSpinnerItem, itemTitles);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;
            ignore_keyset_count++;

            spinner.OnItemSelectedListener = this;

            return spinner;
        }

        public void OnItemSelected(AdapterView parent, View view, int position, long id)
        {
            var selected_title = string.Format("{0}", parent.GetItemAtPosition(position));
            Log.Debug(TAG, "OnItemSelected: " + selected_title);
            if (ignore_keyset_count > 0)
            {
                Log.Debug(TAG, "Ignoring item selection");
                ignore_keyset_count--;
                return;
            }

            foreach (var item in skuDetails)
            {
                if (item.Title.Contains(selected_title))
                {
                    LaunchBilling(item, parent_activity);
                    parent.SetSelection(0);
                    return;
                }
            }
        }

        public void OnNothingSelected(AdapterView parent)
        {
            Log.Debug(TAG, "OnNothingSelected");
        }
    }
}