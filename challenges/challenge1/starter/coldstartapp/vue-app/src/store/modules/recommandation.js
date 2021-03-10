// import axios from 'axios';
// import API from '../config';
// import { parseItem } from './action-utils';
// import { GET_RECOMMANDATION } from './mutation-types';

// const captains = console;

// export default {
//   strict: process.env.NODE_ENV !== 'production',
//   namespaced: true,
//   state: {
//     recommandation: {},
//   },
//   mutations: {
//     [GET_RECOMMANDATION](state, recommandation) {
//       state.recommandation = recommandation;
//     },
//   },
//   actions: {
//     async getRecommandationAction({ commit }) {
//       try {
//         const response = await axios.get(`${API}/recommandation`);
//         const recommandation = parseItem(response);
//         commit(GET_RECOMMANDATION, recommandation);
//         return recommandation;
//       } catch (error) {
//         captains.error(error);
//         throw new Error(error);
//       }
//     },
//   },
//   getters: {
//     recommandation: (state) => state.recommandation,
//   },
// };
