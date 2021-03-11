import axios from 'axios';
import API from '../config';
import { parseList, parseItem } from './action-utils';
import { GET_CATALOG, GET_RECOMMANDATION } from './mutation-types';

const captains = console;

export default {
  strict: process.env.NODE_ENV !== 'production',
  namespaced: true,
  state: {
    catalog: [],
    recommandation: {},
  },
  mutations: {
    [GET_CATALOG](state, catalog) {
      state.catalog = catalog;
    },
    [GET_RECOMMANDATION](state, recommandation) {
      state.recommandation = recommandation;
    },
  },
  actions: {
    async getCatalogAction({ commit }) {
      try {
        const response = await axios.get(`${API}/catalog`);
        const catalog = parseList(response);
        commit(GET_CATALOG, catalog);

        const responseR = await axios.get(`${API}/recommandation`);
        const recommandation = parseItem(responseR, 200);
        const icecreamId = parseInt(recommandation.rewardActionId, 10);
        const result = catalog.filter((obj) => obj.Id === icecreamId)[0];
        result.EventId = recommandation.eventId;
        commit(GET_RECOMMANDATION, result);
      } catch (error) {
        captains.error(error);
        throw new Error(error);
      }
    },
  },
  getters: {
    catalog: (state) => state.catalog,
    recommandation: (state) => state.recommandation,
  },
};
