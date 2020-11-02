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
// const SYNCH_ORDER_URL = '/ship-station/synch-vtex-order/'
// const LIST_HOOKS_URL = '/ship-station/list-active-webhooks'

const initialState = {
  apiKey: '',
  apiSecret: '',
  splitShipmentByLocation: false,
  weightUnit: 'pounds',
}

const weightUnitOptions = [
  { value: 'pounds', label: 'Pounds' },
  { value: 'ounces', label: 'Ounces' },
  { value: 'grams', label: 'Grams' },
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
      splitShipmentByLocation,
      weightUnit,
    } = JSON.parse(data.appSettings?.message || '{}')

    setSettingsState({ apiKey, apiSecret, splitShipmentByLocation, weightUnit })
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
          label="API Key"
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
          label="API Secret"
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
          variation="primary"
          collapseLeft
          onClick={() => {
            handleSetupHooks()
          }}
          disabled={!settingsState.apiKey || !settingsState.apiSecret}
        >
          <FormattedMessage id="admin/ship-station.setup-hooks.button" />
        </Button>
      </div>
    </Layout>
  )
}

export default ShipStationAdmin
