/* eslint-disable padding-line-between-statements */
/* eslint-disable no-console */
import React, { FC } from 'react'
//import { useRuntime } from 'vtex.render-runtime'
//import axios from 'axios'
import {
  Layout,
  PageHeader,
//  Card,
//  Button,
//  ButtonPlain,
//  Divider,
//  Spinner,
} from 'vtex.styleguide'
import { FormattedMessage } from 'react-intl'

//import styles from './styles.css'

//const SETUP_HOOKS_URL = '/ship-station/setup-hooks'
//const SYNCH_ORDER_URL = '/ship-station/synch-vtex-order/'

const AdminExample: FC = () => {
  return (
    <Layout
      pageHeader={
        <PageHeader
          title={<FormattedMessage id="admin/ship-station.title" />}
        />
      }
    >
    </Layout>
  )
}

export default AdminExample
