📢 Use this project, [contribute](https://github.com/vtex-apps/ship-station) to it or open issues to help evolve it using [Store Discussion](https://github.com/vtex-apps/store-discussion).

<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->

[![All Contributors](https://img.shields.io/badge/all_contributors-0-orange.svg?style=flat-square)](#contributors-)

<!-- ALL-CONTRIBUTORS-BADGE:END -->

# ShipStation

The ShipStation app sends orders placed in VTEX to ShipStation.  When the orders are shipped in ShipStation, the shipping information will be updated in VTEX.

## Configuration

### Step 1 - Create API credentials in ShipStation

1. Log in to ShipStation
2. In the upper right hand corner, click the gear icon.
3. In the left hand menu, click Account, then API Settings
4. Generate API Keys.  Record the Key and Secret.

### Step 2 - Create a store

1. In the left-hand column, choose ‘Selling Channels’ and then ‘Store Setup’
2. Click ‘ Connect a Store or Marketplace’ and search for ‘Shipstation’
3. Name the store and click Connect.

### Step 3 - Install the ShipStation app

Using your terminal, log in to the desired VTEX account and run the following command:
`vtex install vtex.ship-station`

### Step 4 - Defining the app settings

1. In the VTEX admin, under Orders, choose Inventory & Shipping, then ShipStation
2. Enter The API Key & Secret from Step 1
3. Choose Weight Unit
4. Choose optional settings
- **Split Shipment by Location** - A separate order for each shipping location will be created in ShipStation
- **Send Pickup In Store Orders to Shipstation** - Orders that are for in store pickup will be sent to ShipStation
- **Only Send Marketplace Orders** - Send only Marketplace orders to ShipStation
- **Include Brand Name and Categories in Item Details** - Show item brand name and categories in Shipstation item details
- **Include Sku Specifications in Item Details** - Show sku specifications in ShipStation item details
- **Update order status to 'Start Handling' when order has been sent to ShipStation** - Change the order status in VTEX to 'Start Handling' when the order has been exported to Shipstation
- **Use Reference Code as Sku** - Use the item Reference Code as sku in ShipStation
- **Show Warehouse Location in Item Details** - Show item warehouse location in Shipstation item details
- **Add Payment Method to Custom field 1** - Show the order payment method in Custom field 1 in ShipStation
5. Save Settings
6. Create Webhooks

## Contributors ✨

Thanks goes to these wonderful people ([emoji key](https://allcontributors.org/docs/en/emoji-key)):

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<!-- markdownlint-enable -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->

This project follows the [all-contributors](https://github.com/all-contributors/all-contributors) specification. Contributions of any kind welcome!
