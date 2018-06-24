<template>
  <div class="contents">
    <h1>Photo Battler</h1>
    <p><router-link to="/">Photo Battler</router-link>は模型の写真から戦闘力を数値化して楽しむサイト……になる予定です。</p>
    <p>現在はそれっぽく数値化するための準備中です。被写体がどのような属性を持っているか？　を学習し、属性ごとの一致度を数値に変換したいと考えています。</p>
    <p><router-link to="/upload">Upload</router-link>から「<i>このモデルはこういう要素を持っている</i>」といった情報を登録していくと、段々準備が進む見込みです。</p>
    <p>飽きたらいつの間にか消滅します。<a href="https://github.com/7474/PhotoBattler/">ソースコードリポジトリはGitHub</a>です。興味がありましたら参照ください。</p>
  </div>
</template>

<script>
export default {
  name: 'HelloWorld'
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
</style>
