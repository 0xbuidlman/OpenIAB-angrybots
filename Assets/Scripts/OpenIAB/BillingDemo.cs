﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OnePF;

public class BillingDemo : MonoBehaviour {
    private const int OFFSET = 5;
    private const int BUTTON_WIDTH = 200;
    private const int BUTTON_HEIGHT = 80;

    private const int SIDE_BUTTON_WIDTH = 140;
    private const int SIDE_BUTTON_HEIGHT = 60;

    private const int WINDOW_WIDTH = 400;
    private const int WINDOW_HEIGHT = 320;

    private const int FONT_SIZE = 24;

    private const int N_ROUNDS = 5;

    public const string SKU_MEDKIT = "sku_medkit";
    public const string SKU_AMMO = "sku_ammo_pack";
    public const string SKU_INFINITE_AMMO = "sku_infinite_ammo";
    public const string SKU_COWBOY_HAT = "sku_cowboy_hat";

    private bool _processingPayment = false;
    private bool _showShopWindow = false;
    private string _popupText = "";

    private GameObject[] _joysticks = null;

    [SerializeField]
    private AmmoBox _playerAmmoBox = null;
    [SerializeField]
    private MedKitPack _playerMedKitPack = null;
    [SerializeField]
    private PlayerHat _playerHat = null;

    private const string STORE_ONEPF = "org.onepf.store";

    #region Billing
    private void Awake() {
        OpenIABEventManager.billingSupportedEvent += OnBillingSupported;
        OpenIABEventManager.billingNotSupportedEvent += OnBillingNotSupported;
        OpenIABEventManager.queryInventorySucceededEvent += OnQueryInventorySucceeded;
        OpenIABEventManager.queryInventoryFailedEvent += OnQueryInventoryFailed;
        OpenIABEventManager.purchaseSucceededEvent += OnPurchaseSucceded;
        OpenIABEventManager.purchaseFailedEvent += OnPurchaseFailed;
        OpenIABEventManager.consumePurchaseSucceededEvent += OnConsumePurchaseSucceeded;
        OpenIABEventManager.consumePurchaseFailedEvent += OnConsumePurchaseFailed;
        OpenIABEventManager.transactionRestoredEvent += OnTransactionRestored;
        OpenIABEventManager.restoreSucceededEvent += OnRestoreSucceeded;
        OpenIABEventManager.restoreFailedEvent += OnRestoreFailed;
    }

    private void Start() {
        // SKU's for iOS MUST be mapped. Mappings for other stores are optional
        OpenIAB.mapSku(SKU_MEDKIT, OpenIAB_iOS.STORE, "30_real");
        OpenIAB.mapSku(SKU_AMMO, OpenIAB_iOS.STORE, "75_real");
        OpenIAB.mapSku(SKU_INFINITE_AMMO, OpenIAB_iOS.STORE, "noncons_2");
        OpenIAB.mapSku(SKU_COWBOY_HAT, OpenIAB_iOS.STORE, "noncons_1");

        OpenIAB.mapSku(SKU_MEDKIT, STORE_ONEPF, "onepf.sku_medkit");
        OpenIAB.mapSku(SKU_AMMO, STORE_ONEPF, "onepf.sku_ammo");
        OpenIAB.mapSku(SKU_COWBOY_HAT, STORE_ONEPF, "onepf.sku_cowboy_hat");
        OpenIAB.mapSku(SKU_INFINITE_AMMO, STORE_ONEPF, "onepf.sku_infinite_ammo");

        var options = new OnePF.Options();
        options.storeKeys.Add(OpenIAB_Android.STORE_GOOGLE, "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAtKiFBiESx95DM1B6acfQVSns4fHR8wwvwzvHTwYac2TWEBdTkvn2tmZwu61NYrp2Puq1qHfgRM2M1pZcfmtUcDwTInP7uD5Gebom8MrOQfC6L8gMj9uksq1MyYq3vhkcHibKhpF47iaLvWsSnzYuwZ0iWGYh71OA2G7S28D1ikQdG+pzJdw9eFi6W+Gmfo0INII30npkNHwxnDv9wZ+eGqvp5M/JqZF3O3p8kjvsUb2IQj7eZlvSAhM/Z2p5XdSi1Os1r2Xu4um0Wv2HcgcbfCJqBwruuZVE+51zyPUMRYkm2/Nv9MyIEHmejvo0wHRmW1iuUzbwgEnkJYxPRqVUKwIDAQAB");
        OpenIAB.init(options);
    }

    private void OnDestroy() {
        OpenIABEventManager.billingSupportedEvent -= OnBillingSupported;
        OpenIABEventManager.billingNotSupportedEvent -= OnBillingNotSupported;
        OpenIABEventManager.queryInventorySucceededEvent -= OnQueryInventorySucceeded;
        OpenIABEventManager.queryInventoryFailedEvent -= OnQueryInventoryFailed;
        OpenIABEventManager.purchaseSucceededEvent -= OnPurchaseSucceded;
        OpenIABEventManager.purchaseFailedEvent -= OnPurchaseFailed;
        OpenIABEventManager.consumePurchaseSucceededEvent -= OnConsumePurchaseSucceeded;
        OpenIABEventManager.consumePurchaseFailedEvent -= OnConsumePurchaseFailed;
        OpenIABEventManager.transactionRestoredEvent -= OnTransactionRestored;
        OpenIABEventManager.restoreSucceededEvent -= OnRestoreSucceeded;
        OpenIABEventManager.restoreFailedEvent -= OnRestoreFailed;
    }

    // Verifies the developer payload of a purchase.
    bool VerifyDeveloperPayload(string developerPayload) {
        /*
         * TODO: verify that the developer payload of the purchase is correct. It will be
         * the same one that you sent when initiating the purchase.
         * 
         * WARNING: Locally generating a random string when starting a purchase and 
         * verifying it here might seem like a good approach, but this will fail in the 
         * case where the user purchases an item on one device and then uses your app on 
         * a different device, because on the other device you will not have access to the
         * random string you originally generated.
         *
         * So a good developer payload has these characteristics:
         * 
         * 1. If two different users purchase an item, the payload is different between them,
         *    so that one user's purchase can't be replayed to another user.
         * 
         * 2. The payload must be such that you can verify it even when the app wasn't the
         *    one who initiated the purchase flow (so that items purchased by the user on 
         *    one device work on other devices owned by the user).
         * 
         * Using your own server to store and verify developer payloads across app
         * installations is recommended.
         */
        return true;
    }

    private void OnBillingSupported() {
        Debug.Log("Billing is supported");
        OpenIAB.queryInventory(new string[] { SKU_AMMO, SKU_COWBOY_HAT, SKU_INFINITE_AMMO, SKU_MEDKIT });
    }

    private void OnBillingNotSupported(string error) {
        Debug.Log("Billing not supported: " + error);
    }

    private void OnQueryInventorySucceeded(Inventory inventory) {
        Debug.Log("Query inventory succeeded: " + inventory);

        // Do we have the infinite ammo subscription?
        Purchase infiniteAmmoPurchase = inventory.GetPurchase(SKU_INFINITE_AMMO);
        bool subscribedToInfiniteAmmo = (infiniteAmmoPurchase != null && VerifyDeveloperPayload(infiniteAmmoPurchase.DeveloperPayload));
        Debug.Log("User " + (subscribedToInfiniteAmmo ? "HAS" : "DOES NOT HAVE") + " infinite ammo subscription.");
        if (subscribedToInfiniteAmmo) {
            _playerAmmoBox.IsInfinite = true;
        }

        // Check cowboy hat purchase
        Purchase cowboyHatPurchase = inventory.GetPurchase(SKU_COWBOY_HAT);
        bool isCowboyHat = (cowboyHatPurchase != null && VerifyDeveloperPayload(cowboyHatPurchase.DeveloperPayload));
        Debug.Log("User " + (isCowboyHat ? "HAS" : "HAS NO") + " cowboy hat");
        _playerHat.PutOn = isCowboyHat;

        // Check for delivery of expandable items. If we own some, we should consume everything immediately
        Purchase medKitPurchase = inventory.GetPurchase(SKU_MEDKIT);
        if (medKitPurchase  != null && VerifyDeveloperPayload(medKitPurchase.DeveloperPayload)) {
            //Debug.Log("We have MedKit. Consuming it.");
            OpenIAB.consumeProduct(inventory.GetPurchase(SKU_MEDKIT));
        }
        Purchase ammoPurchase = inventory.GetPurchase(SKU_AMMO);
        if (ammoPurchase != null && VerifyDeveloperPayload(ammoPurchase.DeveloperPayload)) {
            //Debug.Log("We have ammo. Consuming it.");
            OpenIAB.consumeProduct(inventory.GetPurchase(SKU_AMMO));
        }
    }

    private void OnQueryInventoryFailed(string error) {
        Debug.Log("Query inventory failed: " + error);
    }

    private void OnPurchaseSucceded(Purchase purchase) {
        Debug.Log("Purchase succeded: " + purchase.Sku + "; Payload: " + purchase.DeveloperPayload);
        if (!VerifyDeveloperPayload(purchase.DeveloperPayload)) {
            return;
        }
        switch (purchase.Sku) {
            case SKU_MEDKIT:
                _playerMedKitPack.Supply(1);
                OpenIAB.consumeProduct(purchase);
                return;
            case SKU_AMMO:
                _playerAmmoBox.Supply(N_ROUNDS);
                OpenIAB.consumeProduct(purchase);
                return;
            case SKU_COWBOY_HAT:
                _playerHat.PutOn = true;
                break;
            case SKU_INFINITE_AMMO:
                _playerAmmoBox.IsInfinite = true;
                break;
            default:
                Debug.LogWarning("Unknown SKU: " + purchase.Sku);
                break;
        }
        _processingPayment = false;
    }

    private void OnPurchaseFailed(int errorCode, string error) {
        Debug.Log("Purchase failed: " + error);
        _processingPayment = false;
    }

    private void OnConsumePurchaseSucceeded(Purchase purchase) {
        Debug.Log("Consume purchase succeded: " + purchase.ToString());
        _processingPayment = false;
    }

    private void OnConsumePurchaseFailed(string error) {
        Debug.Log("Consume purchase failed: " + error);
        _processingPayment = false;
    }

    private void OnTransactionRestored(string sku) {
        Debug.Log("Transaction restored: " + sku);
    }

    private void OnRestoreSucceeded() {
        Debug.Log("Transactions restored successfully");
    }

    private void OnRestoreFailed(string error) {
        Debug.Log("Transaction restore failed: " + error);
    }
    #endregion // Billing

    #region GUI
    void DrawPopup(int windowID) {
        // Close button
        if (GUI.Button(new Rect(WINDOW_WIDTH-50, 0, 50, 50), "X")) {
            _popupText = "";
            PauseGame(false);
            ShowJoysticks(true);
        }
        // Text
        GUI.Label(new Rect(10, WINDOW_HEIGHT*0.3f, WINDOW_WIDTH-20, WINDOW_HEIGHT), _popupText);
    }

    void DrawShopWindow(int windowID) {
        // Close button
        if (GUI.Button(new Rect(WINDOW_WIDTH-50, 0, 50, 50), "X")) {
            ShowShopWindow(false);
        }

        if (_processingPayment) {
            GUI.Box(new Rect(10, 40, WINDOW_WIDTH-20, SIDE_BUTTON_HEIGHT), "Processing payment...");
            return;
        }

        GUI.skin.box.alignment = TextAnchor.MiddleCenter;

        // Buy Infinite Ammo subscription
        Rect rect = new Rect(10, 55, WINDOW_WIDTH-20, SIDE_BUTTON_HEIGHT);
        if (_playerAmmoBox.IsInfinite) {
            GUI.Box(rect, "Infinite ammo plan active");
        } else if (GUI.Button(rect, "Buy infinite ammo")) {
            _processingPayment = true;
            OpenIAB.purchaseSubscription(SKU_INFINITE_AMMO);
        }

        // Buy Ammo
        rect = new Rect(10, SIDE_BUTTON_HEIGHT+55, WINDOW_WIDTH-20, SIDE_BUTTON_HEIGHT);
        if (_playerAmmoBox.IsFull) {
            GUI.Box(rect, "Ammo box is full");
        } else if (_playerAmmoBox.IsInfinite) {
            GUI.Box(rect, "Buy Ammo");
        } else if (GUI.Button(rect, string.Format("Buy Ammo ({0} rounds)", N_ROUNDS))) {
            _processingPayment = true;
            OpenIAB.purchaseProduct(SKU_AMMO);
        }

        // Buy MedKit
        rect = new Rect(10, SIDE_BUTTON_HEIGHT*2+55, WINDOW_WIDTH-20, SIDE_BUTTON_HEIGHT);
        if (_playerMedKitPack.IsFull) {
            GUI.Box(rect, "MedKit pack is full");
        } else if (GUI.Button(rect, "Buy MedKit")) {
            _processingPayment = true;
            OpenIAB.purchaseProduct(SKU_MEDKIT);
        }

        // Buy Cowboy Hat
        rect = new Rect(10, SIDE_BUTTON_HEIGHT*3+55, WINDOW_WIDTH-20, SIDE_BUTTON_HEIGHT);
        if (_playerHat.PutOn) {
            GUI.Box(rect, "Cowboy hat purchased");
        } else if (GUI.Button(rect, "Buy Cowboy Hat")) {
            _processingPayment = true;
            OpenIAB.purchaseProduct(SKU_COWBOY_HAT);
        }
    }

    void DrawSidePanel() {
        string[] sideButtons = new string[] {
            _playerAmmoBox.IsInfinite ? "=)" : string.Format("Reload ({0})", _playerAmmoBox.Count),
            string.Format("MedKit ({0})", _playerMedKitPack.Count)
        };

        if (sideButtons.Length <= 0 || _showShopWindow || !string.IsNullOrEmpty(_popupText)) return;

        bool[] buttonClicks = new bool[sideButtons.Length];
        int startY = Screen.height/2-(SIDE_BUTTON_HEIGHT*sideButtons.Length)/2;
        for (int i = 0; i < sideButtons.Length; ++i) {
            buttonClicks[i] = GUI.Button(new Rect(0, startY+SIDE_BUTTON_HEIGHT*i, SIDE_BUTTON_WIDTH, SIDE_BUTTON_HEIGHT), sideButtons[i]);
        }

        // Reload button
        if (buttonClicks[0]) {
            if (_playerAmmoBox.IsInfinite) {
                ShowPopup("No need to reload. Enjoy!");
            } else if (!_playerAmmoBox.Use()) {
                ShowPopup("Sorry, no ammo left. You can buy some in the Shop.");
            }
        }

        // MedKit button
        if (buttonClicks[1]) {
            if (!_playerMedKitPack.Use()) {
                ShowPopup("Sorry, no MedKit's left. You can buy some in the Shop.");
            }
        }
    }

    void ShowPopup(string text) {
        _popupText = text;
        PauseGame(true);
        ShowJoysticks(false);
    }

    void ShowJoysticks(bool show) {
        if (show) {
            foreach (var j in _joysticks) {
                j.SetActive(true);
            }
        } else {
            _joysticks = GameObject.FindGameObjectsWithTag("Joystick");
            foreach (var j in _joysticks) {
                j.SetActive(false);
            }
        }
    }

    void ShowShopWindow(bool show) {
        _showShopWindow = show;
        PauseGame(show);
        ShowJoysticks(!show);
    }

    void OnGUI() {
        GUI.skin.window.fontSize = GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = FONT_SIZE;
        if (!_showShopWindow) {
            if (string.IsNullOrEmpty(_popupText) && GUI.Button(new Rect(Screen.width-BUTTON_WIDTH-OFFSET, OFFSET, BUTTON_WIDTH, BUTTON_HEIGHT), "Shop", GUI.skin.button)) {
                ShowShopWindow(true);
            }
            if (GUI.Button(new Rect(Screen.width-BUTTON_WIDTH-OFFSET, OFFSET*2+BUTTON_HEIGHT, BUTTON_WIDTH, BUTTON_HEIGHT/2), "Restore", GUI.skin.button)) {
                OpenIAB.restoreTransactions();
            }
        } else {
            GUI.Window(0, new Rect(Screen.width/2-WINDOW_WIDTH/2, Screen.height/2-WINDOW_HEIGHT/2, WINDOW_WIDTH, WINDOW_HEIGHT), DrawShopWindow, "Game Shop");
        }

        DrawSidePanel();

        if (!string.IsNullOrEmpty(_popupText)) {
            GUI.Window(0, new Rect(Screen.width/2-WINDOW_WIDTH/2, Screen.height/2-WINDOW_HEIGHT/2, WINDOW_WIDTH, WINDOW_HEIGHT), DrawPopup, "");
        }
    }
    #endregion // GUI

    void PauseGame(bool pause) {
        Time.timeScale = pause ? 0 : 1;
    }
}
