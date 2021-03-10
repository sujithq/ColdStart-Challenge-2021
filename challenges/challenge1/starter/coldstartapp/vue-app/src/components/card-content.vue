<script>
import ButtonFooter from '@/components/button-footer.vue';
import { mapActions, mapGetters } from 'vuex';
import getUserInfo from '@/assets/js/userInfo';

export default {
  name: 'CardContent',
  components: {
    ButtonFooter,
  },
  props: {
    id: {
      type: Number,
      default: () => -1,
    },
    name: {
      type: String,
      default: () => '',
    },
    description: {
      type: String,
      default: () => '',
    },
    imageurl: {
      type: String,
      default: () => '',
    },
    eventId: {
      type: String,
      default: () => null,
    },
    reward: {
      type: Number,
      default: () => 0,
    },
  },
  data() {
    return {
      isAuthenticated: false,
    };
  },
  mounted() {
    this.getIsAuthenticated();
  },
  computed: {
    ...mapGetters('order', { order: 'order' }),
  },
  methods: {
    ...mapActions('order', ['postOrderAction']),
    ...mapActions('reward', ['postRewardAction']),
    async clicked(item) {
      console.log(item);
      console.log(item.id);
      console.log(item.eventId);
      console.log(item.reward);
      if (item.id) {
        const ret = {
          IcecreamId: item.id,
        };
        try {
          await this.postOrderAction(ret);
        } catch (error) {
          console.error(error);
          return false;
        }
      }
      try {
        console.log(`Send Reward ${item.reward}`);
        await this.postRewardAction({ EventId: item.eventId, Reward: item.reward });
      } catch (error) {
        console.error(error);
        return false;
      }
      return true;
    },
    getIsAuthenticated() {
      getUserInfo().then(
        (r) => {
          this.isAuthenticated = Boolean(r && r.identityProvider);
        },
        () => {
          this.isAuthenticated = false;
        },
      );
    },
  },
};
</script>

<template>
  <div class="card-content">
    <header class="card-header">
      <p class="card-header-title">{{ name }}</p>
    </header>

    <div class="content">
      <div class="catalog-image">
        <img v-bind:src="imageurl" />
      </div>
      <p class="description">{{ description }}</p>
      <ButtonFooter
        @clicked="clicked"
        :item="{ id: this.id, eventId: this.eventId, reward: this.reward }"
        label="Add To Cart"
        class="primary"
        v-if="isAuthenticated === true"
      />
    </div>
  </div>
</template>
