<script>
import { mapActions, mapGetters } from 'vuex';
import ListHeader from '@/components/list-header.vue';
import CatalogList from './catalog-list.vue';

export default {
  name: 'Catalog',
  data() {
    return {
      errorMessage: '',
      message: '',
      routePath: '/catalog',
      title: 'Our Ice Creams',
    };
  },
  components: {
    ListHeader,
    CatalogList,
  },
  async created() {
    // console.log('created');
    await this.getCatalog();
    // console.log('getCatalog');
    // await this.getRecommandation();
    // console.log('getRecommandation');
  },
  computed: {
    ...mapGetters('catalog', { catalog: 'catalog' }),
    ...mapGetters('catalog', { recommandation: 'recommandation' }),
  },
  methods: {
    ...mapActions('catalog', ['getCatalogAction']),
    // ...mapActions('recommandation', ['getRecommandationAction']),
    async getCatalog() {
      this.errorMessage = undefined;
      try {
        await this.getCatalogAction();
      } catch (error) {
        this.errorMessage = 'Unauthorized';
      }
    },
    // async getRecommandation() {
    //   this.errorMessage = undefined;
    //   try {
    //     await this.getRecommandationAction();
    //   } catch (error) {
    //     this.errorMessage = 'Unauthorized';
    //   }
    // },
  },
};
</script>

<template>
  <div class="content-container">
    <ListHeader :title="title" @refresh="getCatalog" :routePath="routePath">
    </ListHeader>
    <div class="columns is-multiline is-variable">
      <div class="column" v-if="catalog">
        <CatalogList
          :icecreams="catalog"
          :recommandation="recommandation"
          :errorMessage="errorMessage"
        ></CatalogList>
      </div>
    </div>
  </div>
</template>
