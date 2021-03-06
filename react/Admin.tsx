/* eslint-disable no-console */
import React, { FC, useState, useEffect } from 'react'
import {
  Layout,
  PageHeader,
  Button,
  Input,
  Toggle,
  Dropdown,
} from 'vtex.styleguide'
import { FormattedMessage } from 'react-intl'
import { useQuery, useMutation } from 'react-apollo'

import Settings from './graphql/AppSettings.graphql'
import SaveSettings from './graphql/SaveAppSettings.graphql'

const SETUP_HOOKS_URL = '/ship-station/setup-hooks'
const CREATE_WAREHOUSES_URL = '/ship-station/create-warehouses'

const initialState = {
  apiKey: '',
  apiSecret: '',
  storeName: '',
  brandedReturnsUrl: '',
  splitShipmentByLocation: false,
  sendPickupInStore: false,
  sendItemDetails: false,
  updateOrderStatus: false,
  useRefIdAsSku: false,
  sendSkuDetails: false,
  addDockToOptions: false,
  showPaymentMethod: false,
  weightUnit: 'pounds',
  orderSource: 'fulfillment'
}

const weightUnitOptions = [
  { value: 'pounds', label: 'Pounds' },
  { value: 'ounces', label: 'Ounces' },
  { value: 'grams', label: 'Grams' },
]

const orderSourceOptions = [
    { value: 'fulfillment', label: 'Fulfillment' },
    { value: 'marketplace', label: 'Marketplace' },
]

const ShipStationAdmin: FC = () => {
  const [settingsState, setSettingsState] = useState(initialState)
  const { data } = useQuery(Settings, {
    variables: {
      version: process.env.VTEX_APP_VERSION,
    },
    ssr: false,
  })

  const [saveSettings] = useMutation(SaveSettings)

  // We use useEffect here to save the queried app settings to the component state
  // Basically, whenever the value of 'data' changes, this code will execute
  useEffect(() => {
    if (!data?.appSettings?.message) return

    const {
      apiKey,
      apiSecret,
      storeName,
      brandedReturnsUrl,
      splitShipmentByLocation,
      sendPickupInStore,
      orderSource,
      sendItemDetails,
      updateOrderStatus,
      weightUnit,
      useRefIdAsSku,
      sendSkuDetails,
      addDockToOptions,
      showPaymentMethod
    } = JSON.parse(data.appSettings?.message || '{}')

      setSettingsState({ apiKey, apiSecret, storeName, brandedReturnsUrl, splitShipmentByLocation, sendPickupInStore, orderSource, sendItemDetails, updateOrderStatus, weightUnit, useRefIdAsSku, sendSkuDetails, addDockToOptions, showPaymentMethod })
  }, [data])

  // handler to save new settings by executing the 'saveSettings' mutation
  const handleSaveSettings = () => {
    saveSettings({
      variables: {
        version: process.env.VTEX_APP_VERSION,
        settings: JSON.stringify(settingsState),
      },
    })
  }

  const handleSetupHooks = () => {
    fetch(SETUP_HOOKS_URL)
  }
  // const listHooks = () => {return JSON.stringify(fetch(LIST_HOOKS_URL))}
  const handleCreateWarehouses = () => {
    fetch(CREATE_WAREHOUSES_URL)
  }

  if (!data) return null

  return (
    <Layout
      pageHeader={
        <PageHeader
          title={<FormattedMessage id="admin/ship-station.title" />}
        />
      }
    >
      <div className="mt5">
        <Input
                  label="ShipStation API Key"
          value={settingsState.apiKey}
          onChange={(e: React.FormEvent<HTMLInputElement>) =>
            setSettingsState({
              ...settingsState,
              apiKey: e.currentTarget.value,
            })
          }
        />
      </div>
      <div className="mt5">
        <Input
                  label="ShipStation API Secret"
          value={settingsState.apiSecret}
          onChange={(e: React.FormEvent<HTMLInputElement>) =>
            setSettingsState({
              ...settingsState,
              apiSecret: e.currentTarget.value,
            })
          }
        />
       </div>
     <div className="mt5">
          <Input
               label="ShipStation Store Name (optional)"
               value={settingsState.storeName}
               onChange={(e: React.FormEvent<HTMLInputElement>) =>
                   setSettingsState({
                       ...settingsState,
                       storeName: e.currentTarget.value,
                   })
               }
          />
      </div>
      <div className="mt5">
              <Input
                  label="ShipStation Branded Returns URL"
                  value={settingsState.brandedReturnsUrl}
                  onChange={(e: React.FormEvent<HTMLInputElement>) =>
                      setSettingsState({
                          ...settingsState,
                          brandedReturnsUrl: e.currentTarget.value,
                      })
                  }
              />
      </div>
      <div className="mt5">
              <Dropdown
                  label="Weight Unit"
                  options={weightUnitOptions}
                  value={settingsState.weightUnit}
                  onChange={(_: any, v: string) =>
                      setSettingsState({ ...settingsState, weightUnit: v })
                  }
              />
      </div>
      <div className="mt5">
        <Toggle
          label="Split Shipment by Location"
          size="large"
          checked={settingsState.splitShipmentByLocation}
          onChange={() =>
            setSettingsState({
              ...settingsState,
              splitShipmentByLocation: !settingsState.splitShipmentByLocation,
            })
          }
        />
          </div>
          <div className="mt5">
              <Toggle
                  label="Send Pickup In Store Orders to ShipStation"
                  size="large"
                  checked={settingsState.sendPickupInStore}
                  onChange={() =>
                      setSettingsState({
                          ...settingsState,
                          sendPickupInStore: !settingsState.sendPickupInStore,
                      })
                  }
              />
          </div>
          <div className="mt5">
              <Dropdown
                  label="Order Source"
                  size="large"
                  options={orderSourceOptions}
                  value={settingsState.orderSource}
                  onChange={(_: any, v: string) =>
                      setSettingsState({ ...settingsState, orderSource: v })
                  }
              />
          </div>
          <div className="mt5">
              <Toggle
                  label="Include Brand Name and Categories in Item Details"
                  size="large"
                  checked={settingsState.sendItemDetails}
                  onChange={() =>
                      setSettingsState({
                          ...settingsState,
                          sendItemDetails: !settingsState.sendItemDetails,
                      })
                  }
              />
          </div>
          <div className="mt5">
              <Toggle
                  label="Include Sku Specifications in Item Details"
                  size="large"
                  checked={settingsState.sendSkuDetails}
                  onChange={() =>
                      setSettingsState({
                          ...settingsState,
                          sendSkuDetails: !settingsState.sendSkuDetails,
                      })
                  }
              />
          </div>
          <div className="mt5">
              <Toggle
                  label="Update order status to 'Start Handing' when order has been sent to ShipStation"
                  size="large"
                  checked={settingsState.updateOrderStatus}
                  onChange={() =>
                      setSettingsState({
                          ...settingsState,
                          updateOrderStatus: !settingsState.updateOrderStatus,
                      })
                  }
              />
          </div>
          <div className="mt5">
              <Toggle
                  label="Use Reference Code as Sku"
                  size="large"
                  checked={settingsState.useRefIdAsSku}
                  onChange={() =>
                      setSettingsState({
                          ...settingsState,
                          useRefIdAsSku: !settingsState.useRefIdAsSku,
                      })
                  }
              />
          </div>
          <div className="mt5">
              <Toggle
                  label="Show Warehouse Location in Item Details"
                  size="large"
                  checked={settingsState.addDockToOptions}
                  onChange={() =>
                      setSettingsState({
                          ...settingsState,
                          addDockToOptions: !settingsState.addDockToOptions,
                      })
                  }
              />
          </div>
          <div className="mt5">
              <Toggle
                  label="Add Payment Method to Custom field 1"
                  size="large"
                  checked={settingsState.showPaymentMethod}
                  onChange={() =>
                      setSettingsState({
                          ...settingsState,
                          showPaymentMethod: !settingsState.showPaymentMethod,
                      })
                  }
              />
          </div>
      <div className="mt5">
        <Button
          variation="primary"
          collapseLeft
          onClick={() => {
            handleSaveSettings()
          }}
        >
          <FormattedMessage id="admin/ship-station.save-settings.button" />
        </Button>
      </div>
      <div className="mt5">
        <Button
          variation="secondary"
          collapseLeft
          onClick={() => {
            handleSetupHooks()
          }}
          disabled={!settingsState.apiKey || !settingsState.apiSecret}
        >
          <FormattedMessage id="admin/ship-station.setup-hooks.button" />
        </Button>
      </div>
      <div className="mt5">
        <Button
          variation="secondary"
          collapseLeft
          onClick={() => {
            handleCreateWarehouses()
          }}
        >
          <FormattedMessage id="admin/ship-station.create-warehouses.button" />
        </Button>
      </div>
    </Layout>
  )
}

export default ShipStationAdmin
