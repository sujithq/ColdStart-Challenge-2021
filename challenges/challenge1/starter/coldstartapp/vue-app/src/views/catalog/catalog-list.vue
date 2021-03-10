<script>
import CardContent from '@/components/card-content.vue';

export default {
  name: 'CatalogList',
  props: {
    icecreams: {
      type: Array,
      default: () => [],
    },
    recommandation: {
      type: Object,
      default: () => {},
    },
    errorMessage: {
      type: String,
      default: () => '',
    },
  },
  components: {
    CardContent,
  },
};
</script>

<template>
  <div>
    <div v-if="errorMessage">{{ errorMessage }}</div>
    <div v-if="!icecreams.length && recommandation && !errorMessage">
      Loading data ...
    </div>

    <div class="container">
      <div>
        <div class="card">
          <CardContent
            :id="recommandation.Id"
            :name="recommandation.Name"
            :description="recommandation.Description"
            :imageurl="recommandation.ImageUrl"
            :eventId="recommandation.EventId"
            :reward=1
          />
        </div>
      </div>
    </div>
    <div class="container">
      <div
        v-for="(icecream) in icecreams"
        :key="icecream.Id"
        role="presentation"
      >
        <div class="card">
          <CardContent
            :id="icecream.Id"
            :name="icecream.Name"
            :description="icecream.Description"
            :imageurl="icecream.ImageUrl"
            :eventId="recommandation.EventId"
          />
        </div>
      </div>
    </div>
  </div>
</template>
