/* eslint-disable padding-line-between-statements */
/* eslint-disable no-console */
import React, { FC } from 'react'
//import { useRuntime } from 'vtex.render-runtime'
import axios from 'axios'
import {
  Layout,
  PageHeader,
//  Card,
  Button,
//  ButtonPlain,
//  Divider,
//  Spinner,
} from 'vtex.styleguide'
import { FormattedMessage } from 'react-intl'

//import styles from './styles.css'

import { useQuery } from 'react-apollo'
import Settings from '../graphql/AppSettings.graphql'

const { data } = useQuery(Settings)
const { apiKey } = JSON.parse(data?.appSettings?.ApiKey)
const { apiSecret } = JSON.parse(data?.appSettings?.ApiSecret)

const SETUP_HOOKS_URL = '/ship-station/setup-hooks'
//const SYNCH_ORDER_URL = '/ship-station/synch-vtex-order/'
//const LIST_HOOKS_URL = '/ship-station/list-active-webhooks'

const setupHooks = () => {
    axios
        .get(SETUP_HOOKS_URL)
}

const ShipStationAdmin: FC = () => {
  return (
    <Layout
      pageHeader={
        <PageHeader
          title={<FormattedMessage id="admin/ship-station.title" />}
        />
      }
      >
          <div>
              Api Key {apiKey}
              Api Secret {apiSecret}
          <Button
              variation="primary"
              collapseLeft
              onClick={() => {
                  setupHooks()
              }}
          >
              <FormattedMessage id="admin/ship-station.setup-hooks.button" />
              </Button>
              </div>
    </Layout>
  )
}

export default ShipStationAdmin
