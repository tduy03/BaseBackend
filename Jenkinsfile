pipeline {
    agent any

    environment {
        HARBOR_URL         = 'harbor.ninhbinhic.vn:8089'
        HARBOR_PROJECT     = 'vdcd-hotel'
        BACKEND_IMAGE      = 'backend'
        BACKEND_FULL       = "${HARBOR_URL}/${HARBOR_PROJECT}/${BACKEND_IMAGE}"
        DEPLOY_DIR         = '/opt/vdcd-hotel'
        APP_DIR            = 'backend-hotel'
        BACKEND_SERVICE    = 'backend'
        CONTAINER_NAME     = 'vdcd-hotel-backend'
        DOCKER_BUILDKIT    = '1'
    }

    options {
        buildDiscarder(logRotator(numToKeepStr: '10'))
        timeout(time: 30, unit: 'MINUTES')
        disableConcurrentBuilds()
        timestamps()
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
                script {
                    env.GIT_COMMIT_SHORT = sh(
                        script: "git rev-parse --short HEAD",
                        returnStdout: true
                    ).trim()
                    env.IMAGE_TAG = env.GIT_COMMIT_SHORT
                    echo "Git Commit / Image Tag: ${env.IMAGE_TAG}"
                }
            }
        }

        stage('Sync Code') {
            steps {
                echo "Sync code → ${DEPLOY_DIR}/${APP_DIR}/"
                sh """
                    mkdir -p ${DEPLOY_DIR}/${APP_DIR}
                    rsync -av --delete \
                        --exclude='.git/' \
                        --exclude='*/bin/' \
                        --exclude='*/obj/' \
                        --exclude='*/publish/' \
                        --exclude='VDCD/storage/' \
                        --exclude='VDCD/wwwroot/' \
                        ${WORKSPACE}/ ${DEPLOY_DIR}/${APP_DIR}/
                """
            }
        }

        stage('Build Docker Image') {
            steps {
                echo "Build image: Backend..."
                sh """
                    export DOCKER_BUILDKIT=1
                    cd ${DEPLOY_DIR}

                    docker build \
                        -f ./${APP_DIR}/Dockerfile \
                        -t ${BACKEND_FULL}:${env.IMAGE_TAG} \
                        -t ${BACKEND_FULL}:latest \
                        ./${APP_DIR}/
                """
            }
        }

        stage('Push to Harbor') {
            steps {
                echo "Push image lên Harbor..."
                withCredentials([usernamePassword(
                    credentialsId: 'harbor-credentials',
                    usernameVariable: 'H_USER',
                    passwordVariable: 'H_PASS'
                )]) {
                    sh """
                        echo "\$H_PASS" | docker login ${HARBOR_URL} \
                            -u "\$H_USER" --password-stdin
                        docker push ${BACKEND_FULL}:${env.IMAGE_TAG}
                        docker push ${BACKEND_FULL}:latest
                    """
                }
            }
        }

        stage('Database Migration') {
            steps {
                echo "Chạy EF Core migration..."
                sh """
                    bash ${DEPLOY_DIR}/migrate.sh
                """
            }
        }

        stage('Deploy') {
            steps {
                echo "Pull từ Harbor và Deploy Backend..."
                withCredentials([usernamePassword(
                    credentialsId: 'harbor-credentials',
                    usernameVariable: 'H_USER',
                    passwordVariable: 'H_PASS'
                )]) {
                    sh """
                        echo "\$H_PASS" | docker login ${HARBOR_URL} \
                            -u "\$H_USER" --password-stdin

                        docker pull ${BACKEND_FULL}:${env.IMAGE_TAG}

                        cd ${DEPLOY_DIR}
                        BACKEND_IMAGE=${BACKEND_FULL}:${env.IMAGE_TAG} \
                        docker-compose up -d --no-deps --no-build --force-recreate ${BACKEND_SERVICE}
                    """
                }
            }
        }

        stage('Health Check') {
            steps {
                script {
                    echo "Chờ .NET khởi động (25s)..."
                    sleep(time: 25, unit: 'SECONDS')
                    def status = sh(
                        script: "curl -s -o /dev/null -w '%{http_code}' --max-time 15 http://localhost:5005/ || echo FAIL",
                        returnStdout: true
                    ).trim()
                    echo "Backend Response Code: ${status}"
                    if (status == 'FAIL' || status == '000') {
                        sh "docker logs ${CONTAINER_NAME} --tail=50 || true"
                        error("Backend không phản hồi!")
                    } else {
                        echo "Backend health check passed!"
                    }
                }
            }
        }

        stage('Cleanup') {
            steps {
                sh """
                    docker images ${BACKEND_FULL} --format "{{.ID}}" | tail -n +4 | xargs -r docker rmi || true
                    docker image prune -f || true
                """
            }
        }
    }

    post {
        success {
            echo "VDCD-HOTEL BACKEND DEPLOY THÀNH CÔNG! Build #${env.BUILD_NUMBER} | Image: ${BACKEND_FULL}:${env.IMAGE_TAG}"
        }
        failure {
            echo "VDCD-HOTEL Backend deploy thất bại! Build #${env.BUILD_NUMBER}"
            sh "docker logs ${CONTAINER_NAME} --tail=50 || true"
        }
        always {
            cleanWs(cleanWhenSuccess: true, cleanWhenFailure: false)
        }
    }
}